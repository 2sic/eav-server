using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Loaders;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    public partial class AppState
	{
	    /// <summary>
	    /// All ContentTypes in this App
	    /// </summary>
	    public IEnumerable<IContentType> ContentTypes => _appContentTypesFromRepository.Union(ParentApp.ContentTypes);


        /// <summary>
        /// The second init-command
        /// Load content-types
        /// </summary>
        /// <param name="contentTypes"></param>
        [PrivateApi("should be internal, but ATM also used in FileAppStateLoader")]
        public void InitContentTypes(IList<IContentType> contentTypes)
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
            BuildCacheForTypesByName(_appContentTypesFromRepository);
            //ContentTypesShouldBeReloaded = false;

            l.Done();
        }


        private void BuildCacheForTypesByName(IImmutableList<IContentType> allTypes)
        {
            var l = Log.Fn(message: $"build cache for type names for {allTypes.Count} items", timer: true);

            _appTypesByName = new Dictionary<string, IContentType>(StringComparer.InvariantCultureIgnoreCase);

            var keepTypes = allTypes;

            // add with static name - as the primary key
            foreach (var type in keepTypes)
                if (!_appTypesByName.ContainsKey(type.NameId))
                    _appTypesByName.Add(type.NameId, type);

            // add with nice name, if not already added
            foreach (var type in keepTypes)
                if (!_appTypesByName.ContainsKey(type.Name))
                    _appTypesByName.Add(type.Name, type);

            l.Done();
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
		[PublicApi]
	    public IContentType GetContentType(int contentTypeId)
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
}