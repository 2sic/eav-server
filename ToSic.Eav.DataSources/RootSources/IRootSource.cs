using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.RootSources
{
	/// <summary>
	/// Root data source interface for standard Eav Cache
	/// </summary>
	public interface IRootSource
	{
		/// <summary>
		/// Get a CacheItem to build Cache for this App
		/// </summary>
		AppDataPackage GetDataForCache(IDeferredEntitiesList cacheObjectUsedForDeferredLookups);
		/// <summary>
		/// Get a Dictionary of all Zones and Apps
		/// </summary>
		Dictionary<int, Data.Zone> GetAllZones();
		/// <summary>
		/// Get a Dictionary of all AssignmentObjectTypes
		/// </summary>
		Dictionary<int, string> GetAssignmentObjectTypes();

	    /// <summary>
	    /// Initialize a zone/app combination
	    /// </summary>
	    /// <param name="zoneId"></param>
	    /// <param name="appId"></param>
	    void InitZoneApp(int zoneId, int appId);
	}
}