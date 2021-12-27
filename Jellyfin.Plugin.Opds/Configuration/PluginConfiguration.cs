using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Opds.Configuration;

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
    }

    /// <summary>
    /// Gets or sets a value indicating whether the api should allow anonymous access.
    /// </summary>
    public bool AllowAnonymousAccess { get; set; }
}
