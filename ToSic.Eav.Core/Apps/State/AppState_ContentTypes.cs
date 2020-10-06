using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Types;

namespace ToSic.Eav.Apps
{
    public partial class AppState
	{
	    /// <summary>
	    /// All ContentTypes in this App
	    /// </summary>
	    public IEnumerable<IContentType> ContentTypes 
            => _appTypesFromRepository.Union(Global.AllContentTypes().Values);


	    /// <summary>
	    /// The second init-command
	    /// Load content-types
	    /// </summary>
	    /// <param name="contentTypes"></param>
	    internal void InitContentTypes(IList<IContentType> contentTypes)
        {
            var wrapLog = Log.Call(message: $"init content types {contentTypes?.Count}", useTimer: true);
	        if (!Loading)
	            throw new Exception("trying to set content-types, but not in loading state. set that first!");

            if (Metadata == null || List.Any())
	            throw new Exception("can't set content types before setting Metadata manager, or after entities-list already exists");

	        _appTypeMap = contentTypes
					// temp V11.01 - all the local content-types in the /system/ folder have id=0
					// will filter out for now, because otherwise we get duplicate keys-errors
					// believe this shouldn't be an issue, as it only seems to be used in fairly edge-case export/import
					// situations which the static types shouldn't be used for, as they are json-typed
                .Where(x => x.ContentTypeId != 0)
                .ToImmutableDictionary(x => x.ContentTypeId, x => x.StaticName);
	        _appTypesFromRepository = RemoveAliasesForGlobalTypes(contentTypes);
	        // build types by name
	        BuildCacheForTypesByName(_appTypesFromRepository);
	        ContentTypesShouldBeReloaded = false;
            wrapLog("ok");
        }


        private void BuildCacheForTypesByName(IList<IContentType> allTypes)
        {
            var wrapLog = Log.Call(message: $"build cache for type names for {allTypes.Count} items", useTimer: true);
	        _appTypesByName = new Dictionary<string, IContentType>(StringComparer.InvariantCultureIgnoreCase);

	        var keepTypes = allTypes;

	        // add with static name - as the primary key
	        foreach (var type in keepTypes)
	            if (!_appTypesByName.ContainsKey(type.StaticName))
	                _appTypesByName.Add(type.StaticName, type);

	        // add with nice name, if not already added
	        foreach (var type in keepTypes)
	            if (!_appTypesByName.ContainsKey(type.Name))
	                _appTypesByName.Add(type.Name, type);
            wrapLog("ok");
        }

	    private static ImmutableArray<IContentType> RemoveAliasesForGlobalTypes(IList<IContentType> allTypes)
	    {
	        var globTypeNames = Global.AllContentTypes().Keys;
	        return allTypes.Where(t =>
	                !((ContentType) t).AlwaysShareConfiguration // keep all locally defined types
	                || !globTypeNames.Contains(t.StaticName)    // for non-local: keep all which globally are not overwritten
	            )
	            .ToImmutableArray();
	    }


	    private IDictionary<string, IContentType> _appTypesByName;
	    private ImmutableArray<IContentType> _appTypesFromRepository;
	    private ImmutableDictionary<int, string> _appTypeMap;

        /// <summary>
        /// Get a content-type by name. Will also check global types if needed.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <returns>a type object or null if not found</returns>
	    public IContentType GetContentType(string name)
	        => _appTypesByName.ContainsKey(name)
	            ? _appTypesByName[name]
	            : Global.FindContentType(name); // note: will return null if not found

	    /// <summary>
	    /// Get a content-type by number / id. Will also check global types if needed.
	    /// </summary>
	    /// <param name="contentTypeId">id of the type as stored in the repository</param>
	    /// <returns>a type object or null if not found</returns>
	    public IContentType GetContentType(int contentTypeId)
	    {
            var found = _appTypesFromRepository.FirstOrDefault(c => c.ContentTypeId == contentTypeId);
            if (found != null) return found;

            var name = _appTypeMap.FirstOrDefault(x => x.Key == contentTypeId).Value;
	        return name == null ? null : GetContentType(name);
	    }

        
        [PrivateApi]
	    public bool ContentTypesShouldBeReloaded = true;
	}
}