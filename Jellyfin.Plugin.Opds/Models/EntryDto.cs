#pragma warning disable CA2227
#pragma warning disable CA1002

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Models;

/// <summary>
/// Entry dto.
/// </summary>
[XmlRoot(ElementName = "entry")]
public class EntryDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntryDto"/> class.
    /// </summary>
    protected EntryDto()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryDto"/> class.
    /// </summary>
    /// <param name="title">The entry title.</param>
    /// <param name="id">The entry id.</param>
    /// <param name="updated">The entry update timestamp.</param>
    public EntryDto(
        string title,
        string id,
        DateTime updated)
    {
        Title = title;
        Id = id;
        Updated = updated;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryDto"/> class.
    /// </summary>
    /// <param name="title">The entry title.</param>
    /// <param name="id">The entry id.</param>
    /// <param name="content">The entry content dto.</param>
    /// <param name="updated">The entry update timestamp.</param>
    public EntryDto(
        string title,
        string id,
        ContentDto content,
        DateTime updated)
    {
        Title = title;
        Id = id;
        Content = content;
        Updated = updated;
    }

    /// <summary>
    /// Gets or sets the entry title.
    /// </summary>
    [XmlElement(ElementName = "title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the entry id.
    /// </summary>
    [XmlElement(ElementName = "id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the updated timestamp.
    /// </summary>
    [XmlElement(ElementName = "updated")]
    public DateTime Updated { get; set; }

    /// <summary>
    /// Gets or sets the entry content.
    /// </summary>
    [XmlElement(ElementName = "content")]
    public ContentDto? Content { get; set; }

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    [XmlElement(ElementName="author")]
    public AuthorDto? Author { get; set; }

    /// <summary>
    /// Gets or sets the publisher.
    /// </summary>
    [XmlElement(ElementName="publisher")]
    public PublisherDto? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    [XmlElement(ElementName="language")]
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the summary.
    /// </summary>
    [XmlElement(ElementName="summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the list of links.
    /// </summary>
    [XmlElement(ElementName="link")]
    public List<LinkDto>? Links { get; set; }
}
