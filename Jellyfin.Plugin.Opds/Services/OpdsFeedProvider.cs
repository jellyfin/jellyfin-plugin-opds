using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Opds.Models;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Opds.Services
{
    /// <summary>
    /// OPDS feed provider.
    /// </summary>
    public class OpdsFeedProvider : IOpdsFeedProvider
    {
        private static readonly string[] IncludeItemTypes = { nameof(Book) };
        private static readonly AuthorDto PluginAuthor = new ("Jellyfin", "https://github.com/jellyfin/jellyfin-plugin-opds");

        private readonly ILogger<OpdsFeedProvider> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IServerConfigurationManager _serverConfigurationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpdsFeedProvider"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{OpdsFeedProvider}"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        public OpdsFeedProvider(
            ILogger<OpdsFeedProvider> logger,
            ILibraryManager libraryManager,
            IServerConfigurationManager serverConfigurationManager)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _serverConfigurationManager = serverConfigurationManager;
        }

        /// <inheritdoc />
        public FeedDto GetFeeds()
        {
            var baseUrl = GetBaseUrl();
            return new FeedDto
            {
                Id = Guid.NewGuid().ToString(),
                Links = new[]
                {
                    new LinkDto("self", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;kind=navigation"),
                    new LinkDto("start", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;kind=navigation", "Start"),
                    new LinkDto("search", baseUrl + "/opds/osd", "application/opensearchdescription+xml"),
                    new LinkDto("search", baseUrl + "/opds/search/{searchTerms}", "application/atom+xml", "Search")
                },
                Title = _serverConfigurationManager.Configuration.ServerName,
                Author = PluginAuthor,
                Entries = new List<EntryDto>
                {
                    new ("Alphabetical Books",
                        "/opds/books",
                        new ContentDto("text", "Books sorted alphabetically"),
                        DateTime.UtcNow)
                    {
                        Links = new List<LinkDto>
                        {
                            new (baseUrl + "/opds/books", "application/atom+xml;profile=opds-catalog")
                        }
                    }
                }
            };
        }

        /// <inheritdoc />
        public FeedDto GetAlphabeticalFeed(Guid userId)
        {
            var baseUrl = GetBaseUrl();
            var utcNow = DateTime.UtcNow;
            var entries = new List<EntryDto>
            {
                new (
                    "All",
                    "/opds/books/letter/00",
                    utcNow)
                {
                    Links = new List<LinkDto>
                    {
                        new ("subsection",
                            baseUrl + "/opds/books/letter/00",
                            "application/atom+xml;profile=opds-catalog")
                    }
                }
            };

            // TODO add entries based on library contents first char.
            for (var i = 'A'; i <= 'Z'; i++)
            {
                var letter = char.ToString(i);
                entries.Add(new EntryDto(
                    letter,
                    "/opds/books/letter/" + letter,
                    utcNow)
                {
                    Links = new List<LinkDto>
                    {
                        new ("subsection",
                            baseUrl + "/opds/books/letter/" + letter,
                            "application/atom+xml;profile=opds-catalog")
                    }
                });
            }

            return new FeedDto
            {
                Id = Guid.NewGuid().ToString(),
                Author = PluginAuthor,
                Title = _serverConfigurationManager.Configuration.ServerName,
                Links = new[]
                {
                    new LinkDto("self", baseUrl + "/opds/books?", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                    new LinkDto("start", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                    new LinkDto("up", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                    new LinkDto("search", baseUrl + "/opds/osd", "application/opensearchdescription+xml"),
                    new LinkDto("search", baseUrl + "/opds/search/{searchTerms}", "application/atom+xml", "Search")
                },
                Entries = entries
            };
        }

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <param name="filterStart">The start filter character.</param>
        /// <returns>The list of books.</returns>
        public FeedDto GetAllBooks(string filterStart)
        {
            var baseUrl = GetBaseUrl();
            var books = new List<Book>();
            if (filterStart.Length != 1)
            {
                filterStart = string.Empty;
            }

            foreach (var libraryId in OpdsPlugin.Instance!.Configuration.BookLibraries)
            {
                var items = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    ParentId = libraryId,
                    IncludeItemTypes = IncludeItemTypes,
                    Recursive = true,
                    NameStartsWith = string.IsNullOrEmpty(filterStart) ? null : filterStart
                });

                if (items is null || items.Count == 0)
                {
                    continue;
                }

                foreach (var item in items)
                {
                    if (item is Book book)
                    {
                        books.Add(book);
                    }
                }
            }

            var entries = new List<EntryDto>(books.Count);
            foreach (var book in books)
            {
                entries.Add(new EntryDto(
                    book.Name,
                    book.Id.ToString(),
                    book.DateModified)
                {
                    Author = new AuthorDto
                    {
                        Name = book.Parent.Name
                    },
                    // TODO verify.
                    Summary = book.Overview,
                    Links = new List<LinkDto>
                    {
                        // TODO change type based on actual media type.
                        new ("http://opds-spec.org/image",  baseUrl + "/opds/cover/" + book.Id, "image/jpeg"),
                        new ("http://opds-spec.org/image/thumbnail",  baseUrl + "/opds/cover/" + book.Id, "image/jpeg"),
                        new ("http://opds-spec.org/acquisition", baseUrl + "/opds/download/" + book.Id, "application/epub+zip")
                        {
                            UpdateTime = book.DateModified,
                            Length = book.Size
                        }
                    }
                });
            }

            return new FeedDto
            {
                Id = Guid.NewGuid().ToString(),
                Author = PluginAuthor,
                Title = _serverConfigurationManager.Configuration.ServerName,
                Links = new[]
                {
                    new LinkDto("self", baseUrl + "/opds/books/letter/" + filterStart + "?", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                    new LinkDto("start", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                    new LinkDto("up", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                    new LinkDto("search", baseUrl + "/opds/osd", "application/opensearchdescription+xml"),
                    new LinkDto("search", baseUrl + "/opds/search/{searchTerms}", "application/atom+xml", "Search")
                },
                Entries = entries
            };
        }

        /// <inheritdoc />
        public string? GetBookImage(Guid bookId)
        {
            var item = _libraryManager.GetItemById(bookId);
            return item?.PrimaryImagePath;
        }

        /// <inheritdoc />
        public string? GetBook(Guid bookId)
        {
            var item = _libraryManager.GetItemById(bookId);
            return item?.Path;
        }

        private string GetBaseUrl()
        {
            var baseUrl = _serverConfigurationManager.Configuration.BaseUrl;
            if (string.Equals(baseUrl, "/", StringComparison.Ordinal))
            {
                baseUrl = string.Empty;
            }

            return baseUrl;
        }
    }
}
