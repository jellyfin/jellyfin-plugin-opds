using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Xml.Serialization;

namespace Jellyfin.Plugin.Opds.Services
{
    /// <summary>
    /// Provides a wrapper around third party xml serialization.
    /// </summary>
    public static class XmlHelper
    {
        private static readonly ConcurrentDictionary<string, XmlSerializer> Cache = new ();

        /// <summary>
        /// Gets the XmlSerializer for type and namespace.
        /// </summary>
        /// <param name="type">The serializer type.</param>
        /// <param name="defaultNamespace">The default namespace.</param>
        /// <returns>The created serializer.</returns>
        public static XmlSerializer Create(Type type, string defaultNamespace)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(defaultNamespace))
            {
                throw new ArgumentNullException(nameof(defaultNamespace));
            }

            var key = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", type.FullName!, defaultNamespace);

            return Cache.GetOrAdd(key, _ => new XmlSerializer(type, defaultNamespace));
        }
    }
}
