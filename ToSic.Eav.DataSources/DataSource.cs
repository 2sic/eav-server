﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav
{
	/// <summary>
	/// System to prepare data sources according to our needs.
	/// </summary>
	public class DataSource
	{
	    private const string LogKey = "DS.Factry";

		/// <summary>
		/// Assemble a DataSource with specified Type/Interface-Chain in reversed order.
		/// </summary>
		/// <param name="chain">Array of Full Qualified Names of DataSources</param>
		/// <param name="zoneId">ZoneId for this DataSource</param>
		/// <param name="appId">AppId for this DataSource</param>
		/// <param name="configLookUp">Configuration Provider used for all DataSources</param>
		/// <returns>A single DataSource that has attached </returns>
		private static IDataSource AssembleDataSourceReverse(IList<string> chain, int zoneId, int appId, ILookUpEngine configLookUp)
		{
			var newSource = GetDataSource(chain[0], zoneId, appId, configLookUp: configLookUp);
			if (chain.Count > 1)
			{
				var source = AssembleDataSourceReverse(chain.Skip(1).ToArray(), zoneId, appId, configLookUp);
				((IDataTarget)newSource).Attach(source);
			}
			return newSource;
		}

        private static void Log(ILog log, string method, string message) => log?.Add($"{LogKey}:{method}()'{message}'");

        /// <summary>
	    /// Get DataSource for specified sourceName/Type
	    /// </summary>
	    /// <param name="sourceName">Full Qualified Type/Interface Name</param>
	    /// <param name="zoneId">ZoneId for this DataSource</param>
	    /// <param name="appId">AppId for this DataSource</param>
	    /// <param name="upstream">In-Connection</param>
	    /// <param name="configLookUp">Provides configuration values if needed</param>
	    /// <param name="parentLog"></param>
	    /// <returns>A single DataSource</returns>
	    public static IDataSource GetDataSource(string sourceName, int? zoneId = null, int? appId = null, IDataSource upstream = null, ILookUpEngine configLookUp = null, ILog parentLog = null)
		{
            Log(parentLog, nameof(GetDataSource), $"with name {sourceName}");
		    // try to find with assembly name, or otherwise with GlobalName / previous names
            var type = Type.GetType(sourceName) 
                ?? FindInDsTypeCache(sourceName)?.Type;

		    // still not found? must show error
			if (type == null)
			    throw new Exception("DataSource not installed on Server: " + sourceName);

			return GetDataSource(type, zoneId, appId, upstream, configLookUp, parentLog);
		}


	    /// <summary>
	    /// Get DataSource for specified sourceName/Type
	    /// </summary>
	    /// <param name="type">the .net type of this data-source</param>
	    /// <param name="zoneId">ZoneId for this DataSource</param>
	    /// <param name="appId">AppId for this DataSource</param>
	    /// <param name="upstream">In-Connection</param>
	    /// <param name="configLookUp">Provides configuration values if needed</param>
	    /// <param name="parentLog"></param>
	    /// <returns>A single DataSource</returns>
	    private static IDataSource GetDataSource(Type type, int? zoneId, int? appId, IDataSource upstream,
	        ILookUpEngine configLookUp, ILog parentLog)
	    {
            Log(parentLog, nameof(GetDataSource), "with type");
	        var newDs = (DataSourceBase) Factory.Resolve(type);
	        ConfigureNewDataSource(newDs, zoneId, appId, upstream, configLookUp, parentLog);
	        return newDs;
	    }

	    /// <summary>
	    /// Get DataSource for specified sourceName/Type using Unity.
	    /// </summary>
	    /// <param name="zoneId">ZoneId for this DataSource</param>
	    /// <param name="appId">AppId for this DataSource</param>
	    /// <param name="upstream">In-Connection</param>
	    /// <param name="configLookUp">Provides configuration values if needed</param>
	    /// <param name="parentLog"></param>
	    /// <returns>A single DataSource</returns>
	    public static T GetDataSource<T>(int? zoneId = null, int? appId = null, IDataSource upstream = null,
			ILookUpEngine configLookUp = null, ILog parentLog = null)
		{
            Log(parentLog, nameof(GetDataSource) + $"<{typeof(T).Name}>", $"");
            if (upstream == null && configLookUp == null)
                    throw new Exception("Trying to GetDataSource<T> but cannot do so if both upstream and ConfigurationProvider are null.");
			var newDs = (DataSourceBase)Factory.Resolve(typeof(T));
			ConfigureNewDataSource(newDs, zoneId, appId, upstream, configLookUp ?? upstream.ConfigurationProvider, parentLog);
			return (T)Convert.ChangeType(newDs, typeof(T));
		}

	    /// <summary>
	    /// Helper function (internal) to configure a new data source. This code is used multiple times, that's why it's in an own function
	    /// </summary>
	    /// <param name="newDs">The new data source</param>
	    /// <param name="zoneId">optional Zone #</param>
	    /// <param name="appId">optional app #</param>
	    /// <param name="upstream">upstream data source - for auto-attaching</param>
	    /// <param name="configLookUp">optional configuration provider - for auto-attaching</param>
	    /// <param name="parentLog"></param>
	    private static void ConfigureNewDataSource(DataSourceBase newDs, 
            int? zoneId = null, int? appId = null,
			IDataSource upstream = null,
			ILookUpEngine configLookUp = null, 
            ILog parentLog = null)
		{
            Log(parentLog, nameof(ConfigureNewDataSource), "");
            var zoneAppId = GetZoneAppId(zoneId, appId);
			newDs.ZoneId = zoneAppId.Item1;
			newDs.AppId = zoneAppId.Item2;
			if (upstream != null)
				((IDataTarget)newDs).Attach(upstream);
			if (configLookUp != null)
				newDs.ConfigurationProvider = configLookUp;

            if (parentLog != null)
            {
                Log(parentLog, nameof(ConfigureNewDataSource), "attach log");
                newDs.InitLog(newDs.LogId, parentLog);
            }
		}

		private static readonly string[] InitialDataSourceQuery =
        {
            "ToSic.Eav.DataSources.Caching.IRootCache, ToSic.Eav.DataSources", 
            "ToSic.Eav.DataSources.IRootSource, ToSic.Eav.DataSources"
        };

	    /// <summary>
	    /// Gets a DataSource with Query having PublishingFilter, ICache and IRootSource.
	    /// </summary>
	    /// <param name="zoneId">ZoneId for this DataSource</param>
	    /// <param name="appId">AppId for this DataSource</param>
	    /// <param name="showDrafts">Indicates whether Draft Entities should be returned</param>
	    /// <param name="configProvider"></param>
	    /// <param name="parentLog"></param>
	    /// <returns>A single DataSource</returns>
	    public static IDataSource GetInitialDataSource(int? zoneId = null, int? appId = null, bool showDrafts = false, ILookUpEngine configProvider = null, ILog parentLog = null)
	    {
            parentLog?.AddChild(LogKey, $"get init #{zoneId}/{appId}, draft:{showDrafts}, config:{configProvider != null}");
	        var zoneAppId = GetZoneAppId(zoneId, appId);

			configProvider = configProvider ?? new LookUpEngine();
			var dataSource = AssembleDataSourceReverse(InitialDataSourceQuery, zoneAppId.Item1, zoneAppId.Item2, configProvider);

			var publishingFilter = GetDataSource<PublishingFilter>(zoneAppId.Item1, zoneAppId.Item2, dataSource, configProvider, parentLog);
			publishingFilter.ShowDrafts = showDrafts;

			return publishingFilter;
		}

		/// <summary>
		/// Resolve and validate ZoneId and AppId for specified ZoneId and/or AppId (if any)
		/// </summary>
		/// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		internal static Tuple<int, int> GetZoneAppId(int? zoneId, int? appId)
		{
			if (zoneId == null || appId == null)
			{
                var cache = GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId);
				return cache.GetZoneAppId(zoneId, appId);
			}
			return Tuple.Create(zoneId.Value, appId.Value);
		}

	    private static string _IRootCacheId = "ToSic.Eav.DataSources.Caching.IRootCache, ToSic.Eav.DataSources";

        /// <summary>
        /// Get a new ICache DataSource
        /// </summary>
        /// <param name="zoneId">ZoneId for this DataSource</param>
        /// <param name="appId">AppId for this DataSource</param>
        /// <returns>A new IRootCache</returns>
        public static IRootCache GetCache(int? zoneId, int? appId = null, ILog parentLog = null) 
            => (IRootCache)GetDataSource(_IRootCacheId, zoneId, appId, parentLog:parentLog);

	    /// <summary>
		/// Get DataSource having common MetaData, like Field MetaData
		/// </summary>
		/// <returns>IMetaDataSource (from ICache)</returns>
		public static IMetadataSource GetMetaDataSource(int? zoneId = null, int? appId = null)
		{
			var zoneAppId = GetZoneAppId(zoneId, appId);
			return (IMetadataSource)GetCache(zoneAppId.Item1, zoneAppId.Item2);
		}


        private static DataSourceInfo FindInDsTypeCache(string name)
	        => DsTypeCache
	               .FirstOrDefault(dst => string.Equals(dst.GlobalName, name, StringComparison.InvariantCultureIgnoreCase))
	           ?? DsTypeCache
	               .FirstOrDefault(dst => dst.VisualQuery?
	                                          .PreviousNames.Any(pn => string.Equals(pn, name,
	                                              StringComparison.InvariantCultureIgnoreCase)) ?? false);

	    /// <summary>
	    /// Get all Installed DataSources
	    /// </summary>
	    /// <remarks>Objects that implement IDataSource</remarks>
	    internal static IEnumerable<DataSourceInfo> GetInstalledDataSources(bool onlyForVisualQuery)
	        => onlyForVisualQuery
	            ? DsTypeCache.Where(dsi => !string.IsNullOrEmpty(dsi.VisualQuery?.GlobalName))
	            : DsTypeCache;

        /// <summary>
        /// A cache of all DataSource Types
        /// </summary>
	    private static List<DataSourceInfo> DsTypeCache { get; } = Plumbing.AssemblyHandling
	        .FindInherited(typeof(IDataSource))
	        .Select(t => new DataSourceInfo(t)).ToList();

	}

}