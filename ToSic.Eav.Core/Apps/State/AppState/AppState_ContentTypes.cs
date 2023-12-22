using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.State;

partial class AppState
{
    /// <summary>
    /// All ContentTypes in this App
    /// </summary>
    public IEnumerable<IContentType> ContentTypes => _contentTypesList
        ??= new SynchronizedList<IContentType>(CacheTimestampDelegate,
            () => _appContentTypesFromRepository.Union(ParentApp.ContentTypes).ToImmutableList()
        );
    private SynchronizedList<IContentType> _contentTypesList;

    private IDictionary<string, IContentType> _appTypesByName;
    private IImmutableList<IContentType> _appContentTypesFromRepository;
    private ImmutableDictionary<int, string> _appTypeMap;

    /// <summary>
    /// Get a content-type by name. Will also check global types if needed.
    /// </summary>
    /// <param name="name">name of the type</param>
    /// <returns>a type object or null if not found</returns>
    [PrivateApi("was public till 16.09, but only used on IApp where we now have a similar API")]
    internal IContentType GetContentType(string name) => _appTypesByName.TryGetValue(name, out var type) ? type : ParentApp.GetContentType(name);

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