﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using AppState = ToSic.Eav.Apps.AppState;

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