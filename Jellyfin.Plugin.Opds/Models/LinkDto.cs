using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Models;

/// <summary>
/// The link dto.
/// </summary>
[XmlRoot(ElementName = "link")]
public class LinkDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkDto"/> class.
    /// </summary>
    protected LinkDto()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkDto"/> class.
    /// </summary>
    /// <param name="href">The link href.</param>
    /// <param name="type">The link type.</param>
    public LinkDto(string href, string type)
    {
        Href = href;
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkDto"/> class.
    /// </summary>
    /// <param name="rel">The link rel.</param>
    /// <param name="href">The link href.</param>
    /// <param name="type">The link type.</param>
    public LinkDto(string rel, string href, string type)
    {
        Rel = rel;
        Href = href;
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkDto"/> class.
    /// </summary>
    /// <param name="rel">The link rel.</param>
    /// <param name="href">The link href.</param>
    /// <param name="type">The link type.</param>
    /// <param name="title">The link title.</param>
    public LinkDto(string rel, string href, string type, string title)
    {
        Rel = rel;
        Href = href;
        Type = type;
        Title = title;
    }

    /// <summary>
    /// Gets or sets the link rel.
    /// </summary>
    [XmlAttribute(AttributeName = "rel")]
    public string? Rel { get; set; }

    /// <summary>
    /// Gets or sets the link destination.
    /// </summary>
    [XmlAttribute(AttributeName = "href")]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the link type.
    /// </summary>
    [XmlAttribute(AttributeName = "type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the link title.
    /// </summary>
    [XmlAttribute(AttributeName = "title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    [XmlIgnore]
    public long? Length { get; set; }

    /// <summary>
    /// Gets or sets the string representation of the file size.
    /// </summary>
    /// <remarks>
    /// Xml is unable to serialize "Complex Types", so this is used for serialization.
    /// </remarks>
    [XmlAttribute(AttributeName = "length")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? LengthStr
    {
        get => Length?.ToString(CultureInfo.InvariantCulture);
        set
        {
            switch (value)
            {
                case null:
                    Length = null;
                    break;
                case var l:
                {
                    if (long.TryParse(l, out var val))
                    {
                        Length = val;
                    }

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the book updated time.
    /// </summary>
    [XmlIgnore]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the string representation of the file size.
    /// </summary>
    /// <remarks>
    /// Xml is unable to serialize "Complex Types", so this is used for serialization.
    /// </remarks>
    [XmlAttribute(AttributeName = "mtime")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? UpdateTimeSTr
    {
        get => UpdateTime?.ToString(CultureInfo.InvariantCulture);
        set
        {
            switch (value)
            {
                case null:
                    UpdateTime = null;
                    break;
                case var l:
                {
                    if (DateTime.TryParse(l, out var val))
                    {
                        UpdateTime = val;
                    }

                    break;
                }
            }
        }
    }
}
