using System;
using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Models
{
    /// <summary>
    /// The open search description dto.
    /// </summary>
    [XmlRoot(ElementName = "OpenSearchDescription")]
    public class OpenSearchDescriptionDto
    {
        /// <summary>
        /// Gets or sets the feed long name.
        /// </summary>
        [XmlElement(ElementName = "LongName")]
        public string? LongName { get; set; }

        /// <summary>
        /// Gets or sets the feed short name.
        /// </summary>
        [XmlElement(ElementName = "ShortName")]
        public string? ShortName { get; set; }

        /// <summary>
        /// Gets or sets the feed description.
        /// </summary>
        [XmlElement(ElementName = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the developer name.
        /// </summary>
        [XmlElement(ElementName = "Developer")]
        public string? Developer { get; set; }

        /// <summary>
        /// Gets or sets the contact url.
        /// </summary>
        [XmlElement(ElementName = "Contact")]
        public string? Contact { get; set; }

        /// <summary>
        /// Gets or sets the list of search urls.
        /// </summary>
        [XmlElement(ElementName = "Url")]
        public OpenSearchUrlDto[] Url { get; set; } = Array.Empty<OpenSearchUrlDto>();

        /// <summary>
        /// Gets or sets the syndication right.
        /// </summary>
        [XmlElement(ElementName = "SyndicationRight")]
        public string? SyndicationRight { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        [XmlElement(ElementName = "Language")]
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets the output encoding.
        /// </summary>
        [XmlElement(ElementName = "OutputEncoding")]
        public string? OutputEncoding { get; set; }

        /// <summary>
        /// Gets or sets the input encoding.
        /// </summary>
        [XmlElement(ElementName = "InputEncoding")]
        public string? InputEncoding { get; set; }

        /// <summary>
        /// Gets or sets the xmlns attribute.
        /// </summary>
        [XmlAttribute(AttributeName = "xmlns")]
        public string? Xmlns { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        [XmlText]
        public string? Text { get; set; }
    }
}
