using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Opds.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Opds;

/// <summary>
/// Plugin entrypoint.
/// </summary>
public class OpdsPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly Guid _id = new("F30880AE-3365-449E-B9E6-BF133C8401B0");

    /// <summary>
    /// Initializes a new instance of the <see cref="OpdsPlugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public OpdsPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets current plugin instance.
    /// </summary>
    public static OpdsPlugin? Instance { get; private set; }

    /// <inheritdoc />
    public override Guid Id => _id;

    /// <inheritdoc />
    public override string Name => "OPDS Feed";

    /// <inheritdoc />
    public override string Description => "Provides an OPDS book feed";

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        var prefix = GetType().Namespace;
        yield return new PluginPageInfo
        {
            Name = Name,
            EmbeddedResourcePath = prefix + ".Configuration.Web.config.html"
        };

        yield return new PluginPageInfo
        {
            Name = $"{Name}.js",
            EmbeddedResourcePath = prefix + ".Configuration.Web.config.js"
        };
    }
}
