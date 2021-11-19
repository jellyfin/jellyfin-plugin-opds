using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Jellyfin.Plugin.Opds.Models;
using Jellyfin.Plugin.Opds.Services;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.Opds
{
    /// <summary>
    /// Opds endpoints.
    /// </summary>
    [Route("opds")]
    public class OpdsApi : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IOpdsFeedProvider _opdsFeedProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpdsApi"/> class.
        /// </summary>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        /// <param name="opdsFeedProvider">The opds feed provider.</param>
        public OpdsApi(
            IUserManager userManager,
            OpdsFeedProvider opdsFeedProvider)
        {
            _userManager = userManager;
            _opdsFeedProvider = opdsFeedProvider;
        }

        /// <summary>
        /// Gets the root feed.
        /// </summary>
        /// <returns>The root feed xml.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetRootFeed()
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetFeeds();
            return BuildOutput(feeds);
        }

        /// <summary>
        /// Gets the list of letters to filter by.
        /// </summary>
        /// <returns>The alphabetical feed xml.</returns>
        [HttpGet("books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAlphabeticalRootFeed()
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetAlphabeticalFeed();
            return BuildOutput(feeds);
        }

        /// <summary>
        /// Gets the list of letters to filter by.
        /// </summary>
        /// <param name="startFilter">The start filter.</param>
        /// <returns>The alphabetical feed xml.</returns>
        [HttpGet("books/letter/{startFilter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAlphabeticalFeed(string startFilter)
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetAllBooks(userId, startFilter);
            return BuildOutput(feeds);
        }

        /// <summary>
        /// Gets the search result.
        /// </summary>
        /// <param name="searchTerms">The search terms.</param>
        /// <returns>The search feed xml.</returns>
        [HttpGet("search/{searchTerms}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SearchBookFromRoute(string searchTerms)
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.SearchBooks(userId, searchTerms);
            return BuildOutput(feeds);
        }

        /// <summary>
        /// Gets the search result.
        /// </summary>
        /// <param name="searchTerms">The search terms.</param>
        /// <returns>The search feed xml.</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SearchBookFromQuery([FromQuery] string searchTerms)
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.SearchBooks(userId, searchTerms);
            return BuildOutput(feeds);
        }

        /// <summary>
        /// Gets the search description.
        /// </summary>
        /// <returns>The search description xml.</returns>
        [HttpGet("osd")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetSearchDescription()
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var searchDescription = _opdsFeedProvider.GetSearchDescription();
            return BuildOutput(searchDescription);
        }

        /// <summary>
        /// Gets the book image.
        /// </summary>
        /// <param name="bookId">The book id.</param>
        /// <returns>The book image.</returns>
        [HttpGet("cover/{bookId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetBookImage(Guid bookId)
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var imagePath = _opdsFeedProvider.GetBookImage(bookId);
            if (string.IsNullOrEmpty(imagePath))
            {
                return NotFound();
            }

            return PhysicalFile(imagePath, MimeTypes.GetMimeType(imagePath));
        }

        /// <summary>
        /// Gets the book file.
        /// </summary>
        /// <param name="bookId">The book id.</param>
        /// <returns>The book image.</returns>
        [HttpGet("download/{bookId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DownloadBook(Guid bookId)
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var bookPath = _opdsFeedProvider.GetBook(bookId);
            if (string.IsNullOrEmpty(bookPath))
            {
                return NotFound();
            }

            return PhysicalFile(bookPath, MimeTypes.GetMimeType(bookPath));
        }

        private async Task<Guid> AuthorizeAsync()
        {
            if (OpdsPlugin.Instance!.Configuration.AllowAnonymousAccess)
            {
                // Endpoints don't require auth, allow all requests.
                return Guid.Empty;
            }

            Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader);
            if (authorizationHeader.Count == 0)
            {
                throw new AuthenticationException("Basic Authentication is required");
            }

            var authenticationHeaderValue = AuthenticationHeaderValue.Parse(authorizationHeader);
            if (string.IsNullOrEmpty(authenticationHeaderValue.Parameter)
                || !string.Equals("Basic", authenticationHeaderValue.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new AuthenticationException("Basic Authentication is required");
            }

            var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationHeaderValue.Parameter));
            var credentialSplitIndex = credentialString.IndexOf(':', StringComparison.Ordinal);
            if (credentialSplitIndex < 0)
            {
                throw new AuthenticationException("Basic Authentication is required");
            }

            var username = credentialString[..credentialSplitIndex];
            var password = credentialString[(credentialSplitIndex + 1)..];

            var user = await _userManager.AuthenticateUser(
                    username,
                    password,
                    string.Empty,
                    HttpContext.GetNormalizedRemoteIp().ToString(),
                    true)
                .ConfigureAwait(false);

            if (user is null)
            {
                throw new AuthenticationException("Basic Authentication is required");
            }

            return user.Id;
        }

        private FileStreamResult BuildOutput(FeedDto outputFeed)
        {
            var memoryStream = new MemoryStream();
            var serializer = XmlHelper.Create(typeof(FeedDto), outputFeed.Xmlns);
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, IODefaults.StreamWriterBufferSize, true))
            using (var textWriter = new XmlTextWriter(writer))
            {
                textWriter.Formatting = Formatting.Indented;
                serializer.Serialize(textWriter, outputFeed);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "application/atom+xml; charset=utf-8");
        }

        private FileStreamResult BuildOutput(OpenSearchDescriptionDto outputFeed)
        {
            var memoryStream = new MemoryStream();
            var serializer = XmlHelper.Create(typeof(OpenSearchDescriptionDto), outputFeed.Xmlns!);
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, IODefaults.StreamWriterBufferSize, true))
            using (var textWriter = new XmlTextWriter(writer))
            {
                textWriter.Formatting = Formatting.Indented;
                serializer.Serialize(textWriter, outputFeed);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "application/atom+xml; charset=utf-8");
        }
    }
}
