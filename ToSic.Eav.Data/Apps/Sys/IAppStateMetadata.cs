using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Sys;

public interface IAppStateMetadata
{
    IEntity AppConfiguration { get; }

    /// <summary>
    /// The App-Settings or App-Resources
    /// </summary>
    IEntity MetadataItem { get; }

    IEntity SystemItem { get; }
    IEntity CustomItem { get; }
}