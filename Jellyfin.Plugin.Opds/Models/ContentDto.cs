using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Models
{
    /// <summary>
    /// Content dto.
    /// </summary>
    [XmlRoot(ElementName = "content")]
    public class ContentDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentDto"/> class.
        /// </summary>
        protected ContentDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentDto"/> class.
        /// </summary>
        /// <param name="type">The content type.</param>
        /// <param name="text">The content text.</param>
        public ContentDto(string type, string text)
        {
            Type = type;
            Text = text;
        }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        [XmlAttribute(AttributeName = "type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the content text.
        /// </summary>
        [XmlText]
        public string? Text { get; set; }
    }
}
