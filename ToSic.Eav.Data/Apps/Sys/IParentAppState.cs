using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Sys;

public interface IParentAppState
{
    /// <summary>
    /// The parent App
    /// </summary>
    IAppStateCache AppState { get; }

    /// <summary>
    /// The inherited content-types
    /// </summary>
    IEnumerable<IContentType> ContentTypes { get; }

    /// <summary>
    /// The inherited entities
    /// </summary>
    IEnumerable<IEntity> Entities { get; }
}