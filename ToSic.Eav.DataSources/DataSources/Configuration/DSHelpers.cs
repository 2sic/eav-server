using System;
using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
    internal static class DSHelpers
    {
        public static T Init<T>(this T dataSource, ILookUpEngine lookUp) where T : IDataSource
        {
            if (lookUp != null && dataSource.Configuration is DataSourceConfiguration dsConfig)
                dsConfig.LookUpEngine = lookUp;
            return dataSource;
        }

        /// <summary>
        /// Helper function (internal) to configure a new data source.
        /// </summary>
        /// <param name="dataSource">The new data source</param>
        /// <param name="appIdentity">app identifier</param>
        /// <param name="upstream">upstream data source - for auto-attaching</param>
        /// <param name="lookUp">optional configuration provider - for auto-attaching</param>
        public static T Init<T>(this T dataSource,
            IAppIdentity appIdentity,
            IDataSource upstream = default,
            ILookUpEngine lookUp = default) where T : IDataSource
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));

            if (dataSource is DataSource realDs)
            {
                appIdentity = appIdentity ?? upstream;
                realDs.ZoneId = appIdentity.ZoneId;
                realDs.AppId = appIdentity.AppId;
            }

            if (upstream != null) (dataSource as IDataTarget)?.Attach(upstream);

            lookUp = lookUp ?? dataSource?.Configuration?.LookUpEngine;
            if (lookUp != null && dataSource.Configuration is DataSourceConfiguration dsConfig)
                dsConfig.LookUpEngine = lookUp;
            return dataSource;
        }
    }
}
