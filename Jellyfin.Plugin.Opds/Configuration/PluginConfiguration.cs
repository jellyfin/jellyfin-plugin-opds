using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Opds.Configuration
{
    /// <summary>
    /// Opds plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            AllowAnonymousAccess = false;
            // TODO don't hardcode, allow user to edit library id.
            BookLibraries = new[] { new Guid("4e985111ed7f570b595204d82adb02f3") };
        }

        /// <summary>
        /// Gets or sets a value indicating whether the api should allow anonymous access.
        /// </summary>
        public bool AllowAnonymousAccess { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of book libraries.
        /// </summary>
        public Guid[] BookLibraries { get; set; }
    }
}
