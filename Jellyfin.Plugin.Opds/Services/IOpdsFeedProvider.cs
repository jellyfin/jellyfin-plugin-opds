using System;
using Jellyfin.Plugin.Opds.Models;

namespace Jellyfin.Plugin.Opds.Services
{
    /// <summary>
    /// Opds feed provider.
    /// </summary>
    public interface IOpdsFeedProvider
    {
        /// <summary>
        /// Get the root feeds list.
        /// </summary>
        /// <returns>The root feed.</returns>
        FeedDto GetFeeds();

        /// <summary>
        /// Get the alphabetical books feed.
        /// </summary>
        /// <returns>The alphabetical books feed.</returns>
        FeedDto GetAlphabeticalFeed();

        /// <summary>
        /// Get the list of books matching the filter.
        /// </summary>
        /// <param name="userId">The user id to filter by.</param>
        /// <param name="filterStart">The filter start.</param>
        /// <returns>The list of books.</returns>
        FeedDto GetAllBooks(Guid userId, string filterStart);

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
        /// <param name="userId">The user id to filter by.</param>
        /// <param name="searchTerm">the search term.</param>
        /// <returns>The search result.</returns>
        FeedDto SearchBooks(Guid userId, string searchTerm);

        /// <summary>
        /// Gets the search description.
        /// </summary>
        /// <returns>The search description.</returns>
        OpenSearchDescriptionDto GetSearchDescription();
    }
}
