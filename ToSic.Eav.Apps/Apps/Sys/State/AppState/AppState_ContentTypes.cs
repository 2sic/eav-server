using System.Collections.Immutable;
using ToSic.Sys.Caching.Synchronized;

namespace ToSic.Eav.Apps.Sys.State;

partial class AppState
{
    /// <summary>
    /// All ContentTypes in this App
    /// </summary>
    public IEnumerable<IContentType> ContentTypes => _contentTypesList
        ??= new(CacheTimestampDelegate,
            () =>
            {
                var list = AppContentTypesFromRepository as IEnumerable<IContentType>
                           ?? (_appContentTypesShouldBeLoaded
                               ? throw new NullReferenceException($"Base list of {nameof(ContentTypes)} is empty, but loading is done.")
                               : []);
                return list.Union(ParentApp.ContentTypes).ToImmutableOpt();
            });
    private SynchronizedList<IContentType>? _contentTypesList;

    private IDictionary<string, IContentType> AppTypesByName
    {
        get => field ?? throw new NullReferenceException($"Can't use {nameof(AppTypesByName)} until it's been initialized");
        set;
    }

    private IImmutableList<IContentType>? AppContentTypesFromRepository { get; set; }

    private ImmutableDictionary<int, string> AppTypeMap
    {
        get => field ?? throw new NullReferenceException($"Can't use {nameof(AppTypeMap)} until it's been initialized");
        set;
    }
    private bool _appContentTypesShouldBeLoaded;

    /// <summary>
    /// Get a content-type by name. Will also check global types if needed.
    /// </summary>
    /// <param name="name">name of the type</param>
    /// <returns>a type object or null if not found</returns>
    [PrivateApi("was public till 16.09, but only used on IApp where we now have a similar API")]
    public IContentType? GetContentType(string name) =>
        AppTypesByName.TryGetValue(name, out var type)
            ? type
            : ParentApp.GetContentType(name);

    /// <summary>
    /// Get a content-type by number / id. Will also check global types if needed.
    /// </summary>
    /// <param name="contentTypeId">id of the type as stored in the repository</param>
    /// <returns>a type object or null if not found</returns>
    [PrivateApi("was PublicApi till 16.09 but then it was used on IApp")]
    internal IContentType? GetContentType(int contentTypeId)
    {
        if (AppContentTypesFromRepository == null)
            throw new NullReferenceException($"Error accessing content types for this app, list not initialized.");
        var found = AppContentTypesFromRepository
            .FirstOrDefault(c => c.Id == contentTypeId);
        if (found != null)
            return found;

        var name = AppTypeMap
            .FirstOrDefault(x => x.Key == contentTypeId).Value;
        if (name != null)
            return GetContentType(name);

        // TODO: ONLY do this if #SharedAppFeatureEnabled
        // Try to find in parent
        var parentType = ParentApp.ContentTypes
            .FirstOrDefault(t => t.Id == contentTypeId);
        return parentType;
    }
}