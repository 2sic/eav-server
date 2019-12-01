using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Root data source interface. This is for DataSources that can load entire apps into memory,
	/// so they have additional functionality compared to standard data sources. 
	/// </summary>
	[PrivateApi("not sure about namespace and if this functionality will remain a DataSource")]
	public interface IRootSource
	{
		/// <summary>
		/// Get a CacheItem to build Cache for this App
		/// </summary>
		AppState GetDataForCache(string primaryLanguage = null);

		/// <summary>
		/// Get a Dictionary of all Zones and Apps
		/// </summary>
		Dictionary<int, Zone> GetAllZones();

	    /// <summary>
	    /// Initialize a zone/app combination
	    /// </summary>
	    void InitZoneApp(int zoneId, int appId);
	}
}