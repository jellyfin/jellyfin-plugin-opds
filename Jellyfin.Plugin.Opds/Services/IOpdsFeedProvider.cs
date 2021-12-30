using System;
using Jellyfin.Plugin.Opds.Models;

namespace Jellyfin.Plugin.Opds.Services;

/// <summary>
/// Opds feed provider.
/// </summary>
public interface IOpdsFeedProvider
{
    /// <summary>
    /// Get the root feeds list.
    /// </summary>
    /// <param name="baseUrl">The request path base.</param>
    /// <returns>The root feed.</returns>
    FeedDto GetFeeds(string baseUrl);

    /// <summary>
    /// Get the alphabetical books feed.
    /// </summary>
    /// <param name="baseUrl">The request path base.</param>
    /// <returns>The alphabetical books feed.</returns>
    FeedDto GetAlphabeticalFeed(string baseUrl);

    /// <summary>
    /// Gets the list of book genres.
    /// </summary>
    /// <param name="baseUrl">The request path base.</param>
    /// <param name="userId">The user id to filter by.</param>
    /// <returns>The list of genres.</returns>
    FeedDto GetBookGenres(string baseUrl, Guid userId);

    /// <summary>
    /// Get the list of books matching the filter.
    /// </summary>
    /// <param name="baseUrl">The request path base.</param>
    /// <param name="userId">The user id to filter by.</param>
    /// <param name="filterStart">The filter start.</param>
    /// <returns>The list of books.</returns>
    FeedDto GetAllBooks(string baseUrl, Guid userId, string filterStart);

    /// <summary>
    /// Gets the list of books within a genre.
    /// </summary>
    /// <param name="baseUrl">The request path base.</param>
    /// <param name="userId">The user id to filter by.</param>
    /// <param name="genreId">The genre id.</param>
    /// <returns>The books in the genre.</returns>
    FeedDto GetBooksByGenre(string baseUrl, Guid userId, Guid genreId);

    /// <summary>
    /// Get the book image path.
    /// </summary>
    /// <param name="bookId">The book id.</param>
    /// <returns>The book image path.</returns>
    string? GetBookImage(Guid bookId);

    /// <summary>
    /// Get the book path.
    /// </summary>
    /// <param name="bookId">The book id.</param>
    /// <returns>The book path.</returns>
    string? GetBook(Guid bookId);

    /// <summary>
    /// Searches for a book.
    /// </summary>
    /// <param name="baseUrl">The request path base.</param>
    /// <param name="userId">The user id to filter by.</param>
    /// <param name="searchTerm">the search term.</param>
    /// <returns>The search result.</returns>
    FeedDto SearchBooks(string baseUrl, Guid userId, string searchTerm);

    /// <summary>
    /// Gets the search description.
    /// </summary>
    /// <param name="baseUrl">The request path base.</param>
    /// <returns>The search description.</returns>
    OpenSearchDescriptionDto GetSearchDescription(string baseUrl);
}
