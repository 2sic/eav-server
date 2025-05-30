

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps.Internal.Specs;

/// <summary>
/// Used to deserialize 'editions' from app.json.
///
/// The remaining parts are subclasses, so they don't appear in public namespaces
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppJson
{
    public bool IsConfigured { get; set; }
    public ExportConfig Export { get; set; }
    public DotNetConfig DotNet { get; set; }
    public Dictionary<string, EditionInfo> Editions { get; set; } = [];

    /// <summary>
    /// Export configuration, mainly to define excluded files and folders such as node_modules
    /// </summary>
    public class ExportConfig
    {
        public List<string> Exclude { get; set; }
    }

    /// <summary>
    /// DNN compiler set to roslyn
    /// </summary>
    public class DotNetConfig
    {
        /// <summary>
        /// The compiler to use for Razor and C# classes.
        /// ATM not applied to DataSources and WebApi, as they should be moved to AppCode anyhow
        /// </summary>
        public string Compiler { get; set; }
    }

    /// <summary>
    /// Used to deserialize 'editions' from app.json
    /// </summary>
    public class EditionInfo
    {
        public string Description { get; set; }
        public bool IsDefault { get; set; }
    }
}
