using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Types;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Cache Object for a specific App
    /// </summary>
    public partial class AppState
	{
	    /// <summary>
	    /// Gets all ContentTypes in this App
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
	        Log.Add($"initialize content types for {contentTypes?.Count} types");
	        if (!_loading)
	            throw new Exception("trying to set content-types, but not in loading state. set that first!");

            if (Metadata == null || List.Any())
	            throw new Exception("can't set content types before setting Metadata manager, or after entities-list already exists");

	        _appTypeMap = contentTypes.ToImmutableDictionary(x => x.ContentTypeId, x => x.StaticName);
	        _appTypesFromRepository = RemoveAliasesForGlobalTypes(contentTypes);
	        // build types by name
	        BuildCacheForTypesByName(_appTypesFromRepository);
	        ContentTypesShouldBeReloaded = false;
	    }


        private void BuildCacheForTypesByName(IList<IContentType> allTypes)
        {
            Log.Add($"build cache for type names for {allTypes.Count} items");
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
	    }

	    private static ImmutableList<IContentType> RemoveAliasesForGlobalTypes(IList<IContentType> allTypes)
	    {
	        var globTypeNames = Global.AllContentTypes().Keys;
	        return allTypes.Where(t =>
	                !((ContentType) t).AlwaysShareConfiguration // keep all locally defined types
	                || !globTypeNames.Contains(t.StaticName)    // for non-local: keep all which globally are not overwritten
	            )
	            .ToImmutableList();
	    }


	    private IDictionary<string, IContentType> _appTypesByName;
	    private ImmutableList<IContentType> _appTypesFromRepository;
	    private ImmutableDictionary<int, string> _appTypeMap;

        /// <summary>
        /// Get a content-type by name
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <returns>a type object or null if not found</returns>
	    public IContentType GetContentType(string name)
	        => _appTypesByName.ContainsKey(name)
	            ? _appTypesByName[name]
	            : Global.FindContentType(name); // note: will return null if not found

	    /// <summary>
	    /// Get a content-type by number / id
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

        
	    public bool ContentTypesShouldBeReloaded = true;
	}
}