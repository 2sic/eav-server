namespace ToSic.Eav.Apps.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppStateMetadata
{
    /// <summary>
    /// App Configuration containing the folder etc.
    /// </summary>
    IEntity? AppConfiguration { get; }

    /// <summary>
    /// The App-Settings or App-Resources
    /// </summary>
    IEntity? MetadataItem { get; }

    /// <summary>
    /// The System Settings or System Resources.
    /// </summary>
    IEntity? SystemItem { get; }


    IEntity? CustomItem { get; }
}