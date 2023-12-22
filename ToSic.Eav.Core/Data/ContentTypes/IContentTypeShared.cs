using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a Content Type which is available somewhere, but is defined elsewhere
/// </summary>
[PrivateApi("not yet ready to publish, names will probably change some day")]
public interface IContentTypeShared
{
    /// <summary>
    /// If this configuration is auto-shared everywhere
    /// </summary>
    bool AlwaysShareConfiguration { get; }

}