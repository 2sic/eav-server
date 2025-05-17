using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.State;

public interface IAppStateBuilder: IHasLog
{
    IAppStateBuilder Init(IAppStateCache appState);
    IAppStateBuilder InitForPreset();
    IAppStateBuilder InitForNewApp(ParentAppState? parentApp, IAppIdentity identity, string nameId, ILog parentLog);
    IAppStateCache AppState { get; }
    IAppReader Reader { get; }

    void Load(string message, Action<IAppStateCache> loader);

    void SetNameAndFolder(string name, string folder);

    /// <summary>
    /// Reset all item storages and indexes
    /// </summary>
    void RemoveAllItems();

    /// <summary>
    /// Removes an entity from the cache. Should only be used by EAV code
    /// </summary>
    /// <remarks>
    /// Introduced in v15.05 to reduce work on entity delete.
    /// In past we PurgeApp in whole on each entity delete.
    /// This should be much faster, but side effects are possible.
    /// </remarks>
    void RemoveEntities(int[] repositoryIds, bool log);

    /// <summary>
    /// Add an entity to the cache. Should only be used by EAV code
    /// </summary>
    void Add(IEntity newEntity, int? publishedId, bool log);

    /// <summary>
    /// The first init-command to run after creating the package
    /// it's needed, so the metadata knows what lookup types are supported
    /// </summary>
    void InitMetadata();

    /// <summary>
    /// The second init-command
    /// Load content-types
    /// </summary>
    void InitContentTypes(IList<IContentType> contentTypes);
}