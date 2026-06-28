namespace ToSic.Eav.Apps.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IParentAppState
{
    /// <summary>
    /// The parent App. Can be null on the root app.
    /// </summary>
    IAppStateCache? AppState { get; }

    /// <summary>
    /// The inherited content-types
    /// </summary>
    IEnumerable<IContentType> ContentTypes { get; }

    /// <summary>
    /// The inherited entities
    /// </summary>
    IEnumerable<IEntity> Entities { get; }

    IContentType? GetContentType(string name);
}