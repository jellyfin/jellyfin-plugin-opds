using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Models;

/// <summary>
/// Open search url dto.
/// </summary>
[XmlRoot(ElementName = "Url")]
public class OpenSearchUrlDto
{
    /// <summary>
    /// Gets or sets the link type.
    /// </summary>
    [XmlAttribute(AttributeName = "type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the link template.
    /// </summary>
    [XmlAttribute(AttributeName = "template")]
    public string? Template { get; set; }
}
