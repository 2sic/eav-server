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
                var list = _appContentTypesFromRepository as IEnumerable<IContentType>
                           ?? _emptyContentTypeListDuringLoading
                           ?? throw new NullReferenceException($"Building {nameof(ContentTypes)} but base list is empty, even though loading has already happened");
                return list.Union(ParentApp.ContentTypes).ToImmutableList();
            });
    private SynchronizedList<IContentType> _contentTypesList;

    private IDictionary<string, IContentType> _appTypesByName;
    private IImmutableList<IContentType> _appContentTypesFromRepository;

    /// <summary>
    /// This is a temporary list which should exist before loading, but should be nulled after that.
    /// It's to ensure that the ContentTypes can be accessed during loading (just returning the parents)
    /// but that later would throw an exception if accessed, to avoid confusion.
    /// </summary>
    private List<IContentType> _emptyContentTypeListDuringLoading = [];
    private ImmutableDictionary<int, string> _appTypeMap;

    /// <summary>
    /// Get a content-type by name. Will also check global types if needed.
    /// </summary>
    /// <param name="name">name of the type</param>
    /// <returns>a type object or null if not found</returns>
    [PrivateApi("was public till 16.09, but only used on IApp where we now have a similar API")]
    public IContentType GetContentType(string name) =>
        _appTypesByName.TryGetValue(name, out var type)
            ? type
            : ((ParentAppState)ParentApp).GetContentType(name);

    /// <summary>
    /// Get a content-type by number / id. Will also check global types if needed.
    /// </summary>
    /// <param name="contentTypeId">id of the type as stored in the repository</param>
    /// <returns>a type object or null if not found</returns>
    [PrivateApi("was PublicApi till 16.09 but then it was used on IApp")]
    internal IContentType GetContentType(int contentTypeId)
    {
        var found = _appContentTypesFromRepository.FirstOrDefault(c => c.Id == contentTypeId);
        if (found != null) return found;

        var name = _appTypeMap.FirstOrDefault(x => x.Key == contentTypeId).Value;
        if (name != null) return GetContentType(name);

        // TODO: ONLY do this if #SharedAppFeatureEnabled
        // Try to find in parent
        var parentType = ParentApp.ContentTypes.FirstOrDefault(t => t.Id == contentTypeId);
        return parentType;
    }
}