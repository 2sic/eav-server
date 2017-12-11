﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;

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
		/// <param name="valueCollectionProvider">Configuration Provider used for all DataSources</param>
		/// <returns>A single DataSource that has attached </returns>
		private static IDataSource AssembleDataSourceReverse(IList<string> chain, int zoneId, int appId, IValueCollectionProvider valueCollectionProvider)
		{
			var newSource = GetDataSource(chain[0], zoneId, appId, valueCollectionProvider: valueCollectionProvider);
			if (chain.Count > 1)
			{
				var source = AssembleDataSourceReverse(chain.Skip(1).ToArray(), zoneId, appId, valueCollectionProvider);
				((IDataTarget)newSource).Attach(source);
			}
			return newSource;
		}

	    /// <summary>
	    /// Get DataSource for specified sourceName/Type using Unity.
	    /// </summary>
	    /// <param name="sourceName">Full Qualified Type/Interface Name</param>
	    /// <param name="zoneId">ZoneId for this DataSource</param>
	    /// <param name="appId">AppId for this DataSource</param>
	    /// <param name="upstream">In-Connection</param>
	    /// <param name="valueCollectionProvider">Provides configuration values if needed</param>
	    /// <param name="parentLog"></param>
	    /// <returns>A single DataSource</returns>
	    public static IDataSource GetDataSource(string sourceName, int? zoneId = null, int? appId = null, IDataSource upstream = null, IValueCollectionProvider valueCollectionProvider = null, Log parentLog = null)
		{
			var type = Type.GetType(sourceName);
			if (type == null)
			{
				throw new Exception("DataSource not installed on Server: " + sourceName);
			}
			var newDs = (BaseDataSource)Factory.Resolve(type);
			ConfigureNewDataSource(newDs, zoneId, appId, upstream, valueCollectionProvider, parentLog);
			return newDs;
		}

	    /// <summary>
	    /// Get DataSource for specified sourceName/Type using Unity.
	    /// </summary>
	    /// <param name="zoneId">ZoneId for this DataSource</param>
	    /// <param name="appId">AppId for this DataSource</param>
	    /// <param name="upstream">In-Connection</param>
	    /// <param name="valueCollectionProvider">Provides configuration values if needed</param>
	    /// <param name="parentLog"></param>
	    /// <returns>A single DataSource</returns>
	    public static T GetDataSource<T>(int? zoneId = null, int? appId = null, IDataSource upstream = null,
			IValueCollectionProvider valueCollectionProvider = null, Log parentLog = null)
		{
            if(upstream == null && valueCollectionProvider == null)
                    throw new Exception("Trying to GetDataSource<T> but cannot do so if both upstream and ConfigurationProvider are null.");
			var newDs = (BaseDataSource)Factory.Resolve(typeof(T));
			ConfigureNewDataSource(newDs, zoneId, appId, upstream, valueCollectionProvider ?? upstream.ConfigurationProvider, parentLog);
			return (T)Convert.ChangeType(newDs, typeof(T));
		}

	    /// <summary>
	    /// Helper function (internal) to configure a new data source. This code is used multiple times, that's why it's in an own function
	    /// </summary>
	    /// <param name="newDs">The new data source</param>
	    /// <param name="zoneId">optional Zone #</param>
	    /// <param name="appId">optional app #</param>
	    /// <param name="upstream">upstream data source - for auto-attaching</param>
	    /// <param name="valueCollectionProvider">optional configuration provider - for auto-attaching</param>
	    /// <param name="parentLog"></param>
	    private static void ConfigureNewDataSource(BaseDataSource newDs, int? zoneId = null, int? appId = null,
			IDataSource upstream = null,
			IValueCollectionProvider valueCollectionProvider = null, 
            Log parentLog = null)
		{
			var zoneAppId = GetZoneAppId(zoneId, appId);
			newDs.ZoneId = zoneAppId.Item1;
			newDs.AppId = zoneAppId.Item2;
			if (upstream != null)
				((IDataTarget)newDs).Attach(upstream);
			if (valueCollectionProvider != null)
				newDs.ConfigurationProvider = valueCollectionProvider;

            if(parentLog != null)
                newDs.InitLog(newDs.LogId, parentLog);
		}

		private static readonly string[] InitialDataSourcePipeline = { "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav.DataSources", "ToSic.Eav.DataSources.RootSources.IRootSource, ToSic.Eav.DataSources" };

	    /// <summary>
	    /// Gets a DataSource with Pipeline having PublishingFilter, ICache and IRootSource.
	    /// </summary>
	    /// <param name="zoneId">ZoneId for this DataSource</param>
	    /// <param name="appId">AppId for this DataSource</param>
	    /// <param name="showDrafts">Indicates whehter Draft Entities should be returned</param>
	    /// <param name="configProvider"></param>
	    /// <param name="parentLog"></param>
	    /// <returns>A single DataSource</returns>
	    public static IDataSource GetInitialDataSource(int? zoneId = null, int? appId = null, bool showDrafts = false, IValueCollectionProvider configProvider = null, Log parentLog = null)
	    {
            parentLog?.AddChild(LogKey, $"get init #{zoneId}/{appId}, draft:{showDrafts}, config:{configProvider != null}");
	        var zoneAppId = GetZoneAppId(zoneId, appId);

			configProvider = configProvider ?? new ValueCollectionProvider();
			var dataSource = AssembleDataSourceReverse(InitialDataSourcePipeline, zoneAppId.Item1, zoneAppId.Item2, configProvider);

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

	    private static string _ICacheId = "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav.DataSources";
        /// <summary>
        /// Get a new ICache DataSource
        /// </summary>
        /// <param name="zoneId">ZoneId for this DataSource</param>
        /// <param name="appId">AppId for this DataSource</param>
        /// <returns>A new ICache</returns>
        public static ICache GetCache(int? zoneId, int? appId = null) 
            => (ICache)GetDataSource(_ICacheId, zoneId, appId);

	    /// <summary>
		/// Get DataSource having common MetaData, like Field MetaData
		/// </summary>
		/// <returns>IMetaDataSource (from ICache)</returns>
		public static IMetadataProvider GetMetaDataSource(int? zoneId = null, int? appId = null)
		{
			var zoneAppId = GetZoneAppId(zoneId, appId);
			return (IMetadataProvider)GetCache(zoneAppId.Item1, zoneAppId.Item2);
		}


        // 2017-12-11 2dm - turning this off...
        ///// <summary>
        ///// Get all Installed DataSources
        ///// </summary>
        ///// <remarks>Objects that implement IDataSource</remarks>
        //public static IEnumerable<Type> GetInstalledDataSources(bool onlyForVisualQuery)
        //    => onlyForVisualQuery
        //        ? Plumbing.AssemblyHandling.FindClassesWithAttribute(
        //               typeof(IDataSource),
        //            typeof(VisualQueryAttribute), false)
        //        : Plumbing.AssemblyHandling.FindInherited(typeof(IDataSource));




	    /// <summary>
	    /// Get all Installed DataSources
	    /// </summary>
	    /// <remarks>Objects that implement IDataSource</remarks>
	    public static IEnumerable<DataSourceInfo> GetInstalledDataSources2(bool onlyForVisualQuery)
	        => onlyForVisualQuery
	            ? DsCache.Where(dsi => !string.IsNullOrEmpty(dsi.VisualQuery?.GlobalName))
	            : DsCache;

	    private static List<DataSourceInfo> DsCache { get; } = Plumbing.AssemblyHandling
	        .FindInherited(typeof(IDataSource))
	        .Select(t => new DataSourceInfo(t)).ToList();


	    public class DataSourceInfo
	    {
	        public Type Type { get; }
            public VisualQueryAttribute VisualQuery { get; }
	        public string GlobalName => VisualQuery?.GlobalName;

	        public DataSourceInfo(Type dsType)
	        {
	            Type = dsType;

                // must put this in a try/catch, in case other DLLs have incompatible attributes
	            try
	            {
	                VisualQuery =
	                    Type.GetCustomAttributes(typeof(VisualQueryAttribute), false).FirstOrDefault() as
	                        VisualQueryAttribute;
	            }

                catch {  /*ignore */ }
	        }
	    }
	}

}