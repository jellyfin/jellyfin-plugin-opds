using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Jellyfin.Plugin.Opds.Models;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Search;

namespace Jellyfin.Plugin.Opds.Services
{
    /// <summary>
    /// OPDS feed provider.
    /// </summary>
    public class OpdsFeedProvider : IOpdsFeedProvider
    {
        private static readonly string[] IncludeItemTypes = { nameof(Book) };
        private static readonly AuthorDto PluginAuthor = new ("Jellyfin", "https://github.com/jellyfin/jellyfin-plugin-opds");

        private readonly ILibraryManager _libraryManager;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IUserViewManager _userViewManager;
        private readonly ISearchEngine _searchEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpdsFeedProvider"/> class.
        /// </summary>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        /// <param name="userViewManager">Instance of the <see cref="IUserViewManager"/> interface.</param>
        /// <param name="searchEngine">Instance of the <see cref="ISearchEngine"/> interface.</param>
        public OpdsFeedProvider(
            ILibraryManager libraryManager,
            IServerConfigurationManager serverConfigurationManager,
            IUserViewManager userViewManager,
            ISearchEngine searchEngine)
        {
            _libraryManager = libraryManager;
            _serverConfigurationManager = serverConfigurationManager;
            _userViewManager = userViewManager;
            _searchEngine = searchEngine;
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
                Title = GetServerName(),
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
        public FeedDto GetAlphabeticalFeed()
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
                Title = GetServerName(),
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

        /// <inheritdoc />
        public FeedDto GetAllBooks(Guid userId, string filterStart)
        {
            var baseUrl = GetBaseUrl();
            if (filterStart.Length != 1)
            {
                filterStart = string.Empty;
            }

            Guid[]? libraryIds = null;

            if (userId != Guid.Empty)
            {
                libraryIds = _userViewManager.GetUserViews(new UserViewQuery
                    {
                        IncludeExternalContent = false,
                        UserId = userId
                    })
                    .Select(v => v.Id)
                    .ToArray();
            }

            var entries = new List<EntryDto>();
            foreach (var libraryId in OpdsPlugin.Instance!.Configuration.BookLibraries)
            {
                if (libraryIds is not null && !libraryIds.Contains(libraryId))
                {
                    // user doesn't have permission to view library.
                    continue;
                }

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

                foreach (var item in items.OrderByDescending(item => item.SortName))
                {
                    if (item is Book book)
                    {
                        entries.Add(CreateEntry(book, baseUrl));
                    }
                }
            }

            return new FeedDto
            {
                Id = Guid.NewGuid().ToString(),
                Author = PluginAuthor,
                Title = GetServerName(),
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

        /// <inheritdoc />
        public FeedDto SearchBooks(Guid userId, string searchTerm)
        {
            var baseUrl = GetBaseUrl();
            var searchResult = _searchEngine.GetSearchHints(new SearchQuery
            {
                Limit = 100,
                SearchTerm = searchTerm,
                IncludeItemTypes = IncludeItemTypes,
                UserId = userId
            });

            var entries = new List<EntryDto>(searchResult.Items.Count);
            foreach (var result in searchResult.Items)
            {
                if (result.Item is Book book)
                {
                    entries.Add(CreateEntry(book, baseUrl));
                }
            }

            return new FeedDto
            {
                Id = Guid.NewGuid().ToString(),
                Links = new[]
                {
                    new LinkDto("self", baseUrl + "/opds/search/" + searchTerm + "?", "application/atom+xml;profile=opds-catalog;kind=navigation"),
                    new LinkDto("start", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;kind=navigation", "Start"),
                    new LinkDto("up", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                    new LinkDto("search", baseUrl + "/opds/osd", "application/opensearchdescription+xml"),
                    new LinkDto("search", baseUrl + "/opds/search/{searchTerms}", "application/atom+xml", "Search")
                },
                Title = GetServerName(),
                Author = PluginAuthor,
                Entries = entries
            };
        }

        /// <inheritdoc />
        public OpenSearchDescriptionDto GetSearchDescription()
        {
            var baseUrl = GetBaseUrl();
            var dto = new OpenSearchDescriptionDto
            {
                Xmlns = "http://a9.com/-/spec/opensearch/1.1/",
                Description = "Jellyfin eBook Catalog",
                Developer = "Jellyfin",
                Contact = "https://github.com/jellyfin/jellyfin-plugin-opds",
                SyndicationRight = "open",
                Language = "en-EN",
                OutputEncoding = "UTF-8",
                InputEncoding = "UTF-8",
                ShortName = GetServerName(),
                LongName = GetServerName(),
                Url = new[]
                {
                    new OpenSearchUrlDto
                    {
                        Type = MediaTypeNames.Text.Html,
                        Template = baseUrl + "/opds/search/{searchTerms}"
                    },
                    new OpenSearchUrlDto
                    {
                        Type = "application/atom+xml",
                        Template = baseUrl + "/opds/search?query={searchTerms}"
                    }
                }
            };

            return dto;
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

        private string GetServerName()
        {
            var serverName = _serverConfigurationManager.Configuration.ServerName;
            return string.IsNullOrEmpty(serverName) ? "Jellyfin" : serverName;
        }

        private EntryDto CreateEntry(Book book, string baseUrl)
        {
            var entry = new EntryDto(
                book.Name,
                book.Id.ToString(),
                book.DateModified)
            {
                Author = new AuthorDto
                {
                    Name = book.GetParent().Name
                },
                Summary = book.Overview,
                Links = new List<LinkDto>()
            };

            if (!string.IsNullOrEmpty(book.PrimaryImagePath))
            {
                var imageMimeType = MimeTypes.GetMimeType(book.PrimaryImagePath);
                if (!string.IsNullOrEmpty(imageMimeType))
                {
                    entry.Links.Add(new ("http://opds-spec.org/image", baseUrl + "/opds/cover/" + book.Id, imageMimeType));
                    entry.Links.Add(new ("http://opds-spec.org/image/thumbnail", baseUrl + "/opds/cover/" + book.Id, imageMimeType));
                }
            }

            if (!string.IsNullOrEmpty(book.Path))
            {
                var bookMimeType = MimeTypes.GetMimeType(book.Path);
                if (!string.IsNullOrEmpty(bookMimeType))
                {
                    entry.Links.Add(new ("http://opds-spec.org/acquisition", baseUrl + "/opds/download/" + book.Id, bookMimeType)
                    {
                        UpdateTime = book.DateModified,
                        Length = book.Size
                    });
                }
            }

            return entry;
        }
    }
}
