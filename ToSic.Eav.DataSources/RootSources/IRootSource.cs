using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Data;

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
		AppDataPackage GetDataForCache();

		/// <summary>
		/// Get a Dictionary of all Zones and Apps
		/// </summary>
		Dictionary<int, Zone> GetAllZones();

		/// <summary>
		/// Get a Dictionary of all AssignmentObjectTypes
		/// </summary>
		//Dictionary<int, string> GetAssignmentObjectTypes();

	    /// <summary>
	    /// Initialize a zone/app combination
	    /// </summary>
	    void InitZoneApp(int zoneId, int appId);
	}
}