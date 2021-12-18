using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Models;

/// <summary>
/// Publisher dto.
/// </summary>
[XmlRoot(ElementName = "publisher")]
public class PublisherDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherDto"/> class.
    /// </summary>
    public PublisherDto()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherDto"/> class.
    /// </summary>
    /// <param name="name">The publisher name.</param>
    public PublisherDto(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherDto"/> class.
    /// </summary>
    /// <param name="name">The publisher name.</param>
    /// <param name="uri">The publisher uri.</param>
    public PublisherDto(string name, string uri)
    {
        Name = name;
        Uri = uri;
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [XmlElement(ElementName = "name")]
    public string? Name { get; set;  }

    /// <summary>
    /// Gets or sets the uri.
    /// </summary>
    [XmlElement(ElementName = "uri")]
    public string? Uri { get; set; }
}
