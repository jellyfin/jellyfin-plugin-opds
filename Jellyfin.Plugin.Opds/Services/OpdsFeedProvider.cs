using System;
using System.Collections.Generic;
using System.Net.Mime;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.Opds.Models;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Search;

namespace Jellyfin.Plugin.Opds.Services;

/// <summary>
/// OPDS feed provider.
/// </summary>
public class OpdsFeedProvider : IOpdsFeedProvider
{
    private static readonly BaseItemKind[] BookItemTypes = { BaseItemKind.Book };

    private static readonly AuthorDto PluginAuthor = new("Jellyfin", "https://github.com/jellyfin/jellyfin-plugin-opds");

    private readonly ILibraryManager _libraryManager;
    private readonly ISearchEngine _searchEngine;
    private readonly IServerApplicationHost _serverApplicationHost;
    private readonly IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpdsFeedProvider"/> class.
    /// </summary>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="searchEngine">Instance of the <see cref="ISearchEngine"/> interface.</param>
    /// <param name="serverApplicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    public OpdsFeedProvider(
        ILibraryManager libraryManager,
        ISearchEngine searchEngine,
        IServerApplicationHost serverApplicationHost,
        IUserManager userManager)
    {
        _libraryManager = libraryManager;
        _searchEngine = searchEngine;
        _serverApplicationHost = serverApplicationHost;
        _userManager = userManager;
    }

    /// <inheritdoc />
    public FeedDto GetFeeds(string baseUrl)
    {
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
                new(
                    "Alphabetical Books",
                    "/opds/books",
                    new ContentDto("text", "Books sorted alphabetically"),
                    DateTime.UtcNow)
                {
                    Links = new List<LinkDto>
                    {
                        new(baseUrl + "/opds/books", "application/atom+xml;profile=opds-catalog")
                    }
                },
                new(
                    "Genres",
                    "/opds/genres",
                    new ContentDto("text", "Book genres"),
                    DateTime.UtcNow)
                {
                    Links = new List<LinkDto>
                    {
                        new(baseUrl + "/opds/genres", "application/atom+xml;profile=opds-catalog")
                    }
                }
            }
        };
    }

    /// <inheritdoc />
    public FeedDto GetAlphabeticalFeed(string baseUrl)
    {
        var utcNow = DateTime.UtcNow;
        var entries = new List<EntryDto>
        {
            new(
                "All",
                "/opds/books/letter/00",
                utcNow)
            {
                Links = new List<LinkDto>
                {
                    new(
                        "subsection",
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
                    new(
                        "subsection",
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
    public FeedDto GetBookGenres(string baseUrl, Guid userId)
    {
        var feedDto = new FeedDto
        {
            Id = Guid.NewGuid().ToString(),
            Author = PluginAuthor,
            Title = GetServerName(),
            Links = new[]
            {
                new LinkDto("self", baseUrl + "/opds/genres?", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                new LinkDto("start", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                new LinkDto("up", baseUrl + "/opds", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
                new LinkDto("search", baseUrl + "/opds/osd", "application/opensearchdescription+xml"),
                new LinkDto("search", baseUrl + "/opds/search/{searchTerms}", "application/atom+xml", "Search")
            },
            Entries = new List<EntryDto>()
        };

        var query = new InternalItemsQuery
        {
            IncludeItemTypes = BookItemTypes,
            OrderBy = new (string, SortOrder)[] { ("SortName", SortOrder.Ascending) },
            Recursive = true,
            DtoOptions = new DtoOptions()
        };

        if (userId != Guid.Empty)
        {
            var user = _userManager.GetUserById(userId);
            query.SetUser(user);
        }

        var utcNow = DateTime.UtcNow;
        var queryResult = _libraryManager.GetGenres(query);
        if (queryResult is not null)
        {
            foreach (var (item, _) in queryResult.Items)
            {
                feedDto.Entries.Add(new EntryDto(
                    item.Name,
                    "/opds/genres/" + item.Id,
                    utcNow)
                {
                    Links = new List<LinkDto>
                    {
                        new(
                            "subsection",
                            baseUrl + "/opds/genres/" + item.Id,
                            "application/atom+xml;profile=opds-catalog")
                    }
                });
            }
        }

        return feedDto;
    }

    /// <inheritdoc />
    public FeedDto GetAllBooks(string baseUrl, Guid userId, string filterStart)
    {
        if (filterStart.Length != 1)
        {
            filterStart = string.Empty;
        }

        var query = new InternalItemsQuery
        {
            IncludeItemTypes = BookItemTypes,
            OrderBy = new (string, SortOrder)[] { ("SortName", SortOrder.Ascending) },
            NameStartsWith = filterStart,
            Recursive = true,
            DtoOptions = new DtoOptions()
        };

        if (userId != Guid.Empty)
        {
            var user = _userManager.GetUserById(userId);
            query.SetUser(user);
        }

        var items = _libraryManager.GetItemList(query);
        var entries = new List<EntryDto>();
        if (items is not null)
        {
            foreach (var item in items)
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
    public FeedDto GetBooksByGenre(string baseUrl, Guid userId, Guid genreId)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = BookItemTypes,
            OrderBy = new (string, SortOrder)[] { ("SortName", SortOrder.Ascending) },
            GenreIds = new[] { genreId },
            DtoOptions = new DtoOptions()
        };

        if (userId != Guid.Empty)
        {
            var user = _userManager.GetUserById(userId);
            query.SetUser(user);
        }

        var queryResult = _libraryManager.GetItemList(query);
        var entries = new List<EntryDto>();
        if (queryResult is not null)
        {
            foreach (var item in queryResult)
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
                new LinkDto("self", baseUrl + "/opds/genres/" + genreId + "?", "application/atom+xml;profile=opds-catalog;type=feed;kind=navigation"),
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
    public FeedDto SearchBooks(string baseUrl, Guid userId, string searchTerm)
    {
        var searchResult = _searchEngine.GetSearchHints(new SearchQuery
        {
            Limit = 100,
            SearchTerm = searchTerm,
            IncludeItemTypes = BookItemTypes,
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
    public OpenSearchDescriptionDto GetSearchDescription(string baseUrl)
    {
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

    private string GetServerName()
    {
        var serverName = _serverApplicationHost.FriendlyName;
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
                entry.Links.Add(new("http://opds-spec.org/image", baseUrl + "/opds/cover/" + book.Id, imageMimeType));
                entry.Links.Add(new("http://opds-spec.org/image/thumbnail", baseUrl + "/opds/cover/" + book.Id, imageMimeType));
            }
        }

        if (!string.IsNullOrEmpty(book.Path))
        {
            var bookMimeType = MimeTypes.GetMimeType(book.Path);
            if (!string.IsNullOrEmpty(bookMimeType))
            {
                entry.Links.Add(new("http://opds-spec.org/acquisition", baseUrl + "/opds/download/" + book.Id, bookMimeType)
                {
                    UpdateTime = book.DateModified,
                    Length = book.Size
                });
            }
        }

        return entry;
    }
}
