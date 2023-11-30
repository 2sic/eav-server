using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Loaders;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps;

partial class AppState: IAppContentTypeReader
{
    /// <summary>
    /// All ContentTypes in this App
    /// </summary>
    public IEnumerable<IContentType> ContentTypes => _contentTypesList
        ??= new SynchronizedList<IContentType>(CacheTimestampDelegate,
            () => _appContentTypesFromRepository.Union(ParentApp.ContentTypes).ToImmutableList()
        );
    private SynchronizedList<IContentType> _contentTypesList;

    /// <summary>
    /// The second init-command
    /// Load content-types
    /// </summary>
    /// <param name="contentTypes"></param>
    [PrivateApi("should be internal, but ATM also used in FileAppStateLoader")]
    internal void InitContentTypes(IList<IContentType> contentTypes)
    {
        var l = Log.Fn($"contentTypes count: {contentTypes?.Count}", timer: true);

        if (!Loading)
            throw new Exception("trying to set content-types, but not in loading state. set that first!");

        if (_metadataManager == null || Index.Any())
            throw new Exception(
                "can't set content types before setting Metadata manager, or after entities-list already exists");

        _appTypeMap = contentTypes
            // temp V11.01 - all the local content-types in the /system/ folder have id=0
            // will filter out for now, because otherwise we get duplicate keys-errors
            // believe this shouldn't be an issue, as it only seems to be used in fairly edge-case export/import
            // situations which the static types shouldn't be used for, as they are json-typed
            .Where(x => x.Id != 0 && x.Id < FsDataConstants.GlobalContentTypeMin)
            .ToImmutableDictionary(x => x.Id, x => x.NameId);
        _appContentTypesFromRepository = RemoveAliasesForGlobalTypes(contentTypes);
        // build types by name
        _appTypesByName = BuildCacheForTypesByName(_appContentTypesFromRepository, Log);
        //ContentTypesShouldBeReloaded = false;

        l.Done();
    }


    private static IDictionary<string, IContentType> BuildCacheForTypesByName(IImmutableList<IContentType> allTypes, ILog log)
    {
        var l = log.Fn< IDictionary<string, IContentType>>(message: $"build cache for type names for {allTypes.Count} items", timer: true);

        var appTypesByName = new Dictionary<string, IContentType>(StringComparer.InvariantCultureIgnoreCase);

        // add with static name - as the primary key
        foreach (var type in allTypes)
            if (!appTypesByName.ContainsKey(type.NameId))
                appTypesByName.Add(type.NameId, type);

        // add with nice name, if not already added
        foreach (var type in allTypes)
            if (!appTypesByName.ContainsKey(type.Name))
                appTypesByName.Add(type.Name, type);

        return l.Return(appTypesByName);
    }

    private IImmutableList<IContentType> RemoveAliasesForGlobalTypes(IList<IContentType> appTypes)
    {
        var globTypeNames = ParentApp.ContentTypes.Select(t => t.NameId);
        return appTypes.Where(t =>
                    !t.AlwaysShareConfiguration // keep all locally defined types
                    || !globTypeNames.Contains(t.NameId)    // for non-local: keep all which globally are not overwritten
            )
            .ToImmutableList();
    }


    private IDictionary<string, IContentType> _appTypesByName;
    private IImmutableList<IContentType> _appContentTypesFromRepository;
    private ImmutableDictionary<int, string> _appTypeMap;

    /// <summary>
    /// Get a content-type by name. Will also check global types if needed.
    /// </summary>
    /// <param name="name">name of the type</param>
    /// <returns>a type object or null if not found</returns>
    [PublicApi]
    public IContentType GetContentType(string name)
        => _appTypesByName.ContainsKey(name)
            ? _appTypesByName[name]
            : ParentApp.GetContentType(name);

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