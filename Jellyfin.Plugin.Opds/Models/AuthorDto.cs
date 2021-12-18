using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Models;

/// <summary>
/// Author dto.
/// </summary>
[XmlRoot(ElementName = "author")]
public class AuthorDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorDto"/> class.
    /// </summary>
    public AuthorDto()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorDto"/> class.
    /// </summary>
    /// <param name="name">The author name.</param>
    public AuthorDto(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorDto"/> class.
    /// </summary>
    /// <param name="name">The author name.</param>
    /// <param name="uri">The author uri.</param>
    public AuthorDto(string name, string uri)
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
