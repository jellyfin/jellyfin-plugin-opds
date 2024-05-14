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

namespace Jellyfin.Plugin.Opds;

/// <summary>
/// Opds endpoints.
/// </summary>
[Route("opds")]
public class OpdsApi : ControllerBase
{
    private const string AuthHeaderKey = "WWW-Authenticate";
    private const string AuthHeaderValue = "Basic realm=\"Login Required\"";
    private readonly IUserManager _userManager;
    private readonly IOpdsFeedProvider _opdsFeedProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpdsApi"/> class.
    /// </summary>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="opdsFeedProvider">The opds feed provider.</param>
    public OpdsApi(
        IUserManager userManager,
        IOpdsFeedProvider opdsFeedProvider)
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
        try
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetFeeds(Request.PathBase);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the list of letters to filter by.
    /// </summary>
    /// <returns>The alphabetical feed xml.</returns>
    [HttpGet("Books")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAlphabeticalRootFeed()
    {
        try
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetAlphabeticalFeed(Request.PathBase);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the list of genres.
    /// </summary>
    /// <returns>The genres feed xml.</returns>
    [HttpGet("Genres")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetGenres()
    {
        try
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetBookGenres(Request.PathBase, userId);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the list of recently added books.
    /// </summary>
    /// <returns>The recently added feed xml.</returns>
    [HttpGet("Books/RecentlyAdded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRecentlyAdded()
    {
        try
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetRecentlyAdded(Request.PathBase, userId);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the list of favorite books.
    /// </summary>
    /// <returns>The recently added feed xml.</returns>
    [HttpGet("Books/Favorite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetFavoriteBooks()
    {
        try
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetFavoriteBooks(Request.PathBase, userId);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the list of letters to filter by.
    /// </summary>
    /// <param name="startFilter">The start filter.</param>
    /// <returns>The alphabetical feed xml.</returns>
    [HttpGet("Books/Letter/{startFilter}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAlphabeticalFeed([FromRoute] string startFilter)
    {
        try
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetAllBooks(Request.PathBase, userId, startFilter);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the list of books in the genre.
    /// </summary>
    /// <param name="genreId">The genre id.</param>
    /// <returns>The books feed xml.</returns>
    [HttpGet("Genres/{genreId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetBooksByGenre([FromRoute] Guid genreId)
    {
        try
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.GetBooksByGenre(Request.PathBase, userId, genreId);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the search result.
    /// </summary>
    /// <param name="searchTerms">The search terms.</param>
    /// <returns>The search feed xml.</returns>
    [HttpGet("Search/{searchTerms}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SearchBookFromRoute(string searchTerms)
    {
        try
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.SearchBooks(Request.PathBase, userId, searchTerms);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the search result.
    /// For backwards compatibility, there are multiple accepted query
    /// parameters for the search terms.
    /// </summary>
    /// <param name="searchTerms">The search terms as historically used by this plugin.</param>
    /// <param name="q">The search terms as defined by the OPDS spec.</param>
    /// <param name="query">The search terms as some OPDS clients provide them.</param>
    /// <returns>The search feed xml.</returns>
    [HttpGet("Search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SearchBookFromQuery(
        [FromQuery] string? searchTerms = null,
        [FromQuery] string? q = null,
        [FromQuery] string? query = null)
    {
        searchTerms ??= q ?? query;

        // None of the query parameters was given, but we need at least one, so
        // it's a Bad Request.
        if (searchTerms == null)
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        try
        {
            var userId = await AuthorizeAsync().ConfigureAwait(false);
            var feeds = _opdsFeedProvider.SearchBooks(Request.PathBase, userId, searchTerms);
            return BuildOutput(feeds);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the search description.
    /// </summary>
    /// <returns>The search description xml.</returns>
    [HttpGet("osd")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSearchDescription()
    {
        try
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var searchDescription = _opdsFeedProvider.GetSearchDescription(Request.PathBase);
            return BuildOutput(searchDescription);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the book image.
    /// </summary>
    /// <param name="bookId">The book id.</param>
    /// <returns>The book image.</returns>
    [HttpGet("Cover/{bookId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetBookImage(Guid bookId)
    {
        try
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var imagePath = _opdsFeedProvider.GetBookImage(bookId);
            if (string.IsNullOrEmpty(imagePath))
            {
                return NotFound();
            }

            return PhysicalFile(imagePath, MimeTypes.GetMimeType(imagePath));
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Gets the book file.
    /// </summary>
    /// <param name="bookId">The book id.</param>
    /// <returns>The book image.</returns>
    [HttpGet("Download/{bookId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadBook(Guid bookId)
    {
        try
        {
            await AuthorizeAsync().ConfigureAwait(false);
            var bookPath = _opdsFeedProvider.GetBook(bookId);
            if (string.IsNullOrEmpty(bookPath))
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(bookPath);
            return PhysicalFile(bookPath, MimeTypes.GetMimeType(bookPath), fileName);
        }
        catch (AuthenticationException)
        {
            Response.Headers.Append(AuthHeaderKey, AuthHeaderValue);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }

    private async Task<Guid> AuthorizeAsync()
    {
        var allowAnonymous = OpdsPlugin.Instance!.Configuration.AllowAnonymousAccess;

        Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader);
        if (string.IsNullOrEmpty(authorizationHeader) || authorizationHeader.Count == 0)
        {
            if (allowAnonymous)
            {
                return Guid.Empty;
            }

            throw new AuthenticationException("Basic Authentication is required");
        }

        var authenticationHeaderValue = AuthenticationHeaderValue.Parse(authorizationHeader!);
        if (string.IsNullOrEmpty(authenticationHeaderValue.Parameter)
            || !string.Equals("Basic", authenticationHeaderValue.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            if (allowAnonymous)
            {
                return Guid.Empty;
            }

            throw new AuthenticationException("Basic Authentication is required");
        }

        var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationHeaderValue.Parameter));
        var credentialSplitIndex = credentialString.IndexOf(':', StringComparison.Ordinal);
        if (credentialSplitIndex < 0)
        {
            if (allowAnonymous)
            {
                return Guid.Empty;
            }

            throw new AuthenticationException("Basic Authentication is required");
        }

        var username = credentialString[..credentialSplitIndex];
        var password = credentialString[(credentialSplitIndex + 1)..];

        var user = await _userManager.AuthenticateUser(
                username,
                password,
                string.Empty,
                HttpContext.GetNormalizedRemoteIP().ToString(),
                true)
            .ConfigureAwait(false);

        if (user is null)
        {
            if (allowAnonymous)
            {
                return Guid.Empty;
            }

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
