using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Opds.Models;
using Jellyfin.Plugin.Opds.Services;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Serialization;
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
        private readonly IXmlSerializer _xmlSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpdsApi"/> class.
        /// </summary>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        /// <param name="opdsFeedProvider">The opds feed provider.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public OpdsApi(
            IUserManager userManager,
            OpdsFeedProvider opdsFeedProvider,
            IXmlSerializer xmlSerializer)
        {
            _userManager = userManager;
            _opdsFeedProvider = opdsFeedProvider;
            _xmlSerializer = xmlSerializer;
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
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetAlphabeticalFeed(userId);
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
            await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetAllBooks(startFilter);
            return BuildOutput(feeds);
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

            return PhysicalFile(imagePath, "image/jpeg");
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

            return PhysicalFile(bookPath, "application/epub+zip");
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
            var credentialSplitIndex = credentialString.IndexOf(':');
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
                    HttpContext.GetNormalizedRemoteIp(),
                    true)
                .ConfigureAwait(false);

            if (user is null)
            {
                throw new AuthenticationException("Basic Authentication is required");
            }

            return user.Id;
        }

        private FileStreamResult BuildOutput(FeedDto feedDto)
        {
            var memoryStream = new MemoryStream();
            _xmlSerializer.SerializeToStream(feedDto, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "application/atom+xml; charset=utf-8");
        }
    }
}
