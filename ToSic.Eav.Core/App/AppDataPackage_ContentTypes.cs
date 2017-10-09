using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types;

namespace ToSic.Eav.App
{
	/// <inheritdoc />
	/// <summary>
	/// Cache Object for a specific App
	/// </summary>
	public partial class AppDataPackage
	{
	    /// <summary>
	    /// Gets all ContentTypes in this App
	    /// </summary>
	    //public IDictionary<int, IContentType> ContentTypes { get; }

	    // todo: try replace all access to "ContentTypes" to this, and then rename to ContentTypes
	    public IEnumerable<IContentType> ContentTypes => _appTypesFromRepository.Values.Union(Global.SystemContentTypes().Values);

	    private void BuildTypesByNameCache(IDictionary<int, IContentType> allTypes)
	    {
	        _appTypesByName = new Dictionary<string, IContentType>(StringComparer.InvariantCultureIgnoreCase);

	        // add with static name - as the primary key
	        foreach (var type in allTypes)
	            if (!_appTypesByName.ContainsKey(type.Value.StaticName))
	                _appTypesByName.Add(type.Value.StaticName, type.Value);

	        // add with nice name, if not already added
	        foreach (var type in allTypes)
	            if (!_appTypesByName.ContainsKey(type.Value.Name))
	                _appTypesByName.Add(type.Value.Name, type.Value);
	    }



	    private IDictionary<string, IContentType> _appTypesByName;
	    private readonly IDictionary<int, IContentType> _appTypesFromRepository;

        /// <summary>
        /// Get a content-type by name
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <returns>a type object or null if not found</returns>
	    public IContentType GetContentType(string name)
	        => _appTypesByName.ContainsKey(name)
	            ? _appTypesByName[name]
	            : Global.SystemContentType(name); // note: will return null if not found

        /// <summary>
        /// Get a content-type by number / id
        /// </summary>
        /// <param name="contentTypeId">id of the type as stored in the repository</param>
        /// <returns>a type object or null if not found</returns>
	    public IContentType GetContentType(int contentTypeId) => _appTypesFromRepository.FirstOrDefault(c => c.Key == contentTypeId).Value;
    }
}