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
        /// <param name="thisDs">The new data source</param>
        /// <param name="appIdentity">app identifier</param>
        /// <param name="source">upstream data source - for auto-attaching</param>
        /// <param name="lookUp">optional configuration provider - for auto-attaching</param>
        public static T Init<T>(this T thisDs,
            IAppIdentity appIdentity,
            IDataSource source = default,
            ILookUpEngine lookUp = default) where T : IDataSource
        {
            if (thisDs == null) throw new ArgumentNullException(nameof(thisDs));

            if (thisDs is DataSource realDs)
            {
                appIdentity = appIdentity ?? source;
                realDs.ZoneId = appIdentity.ZoneId;
                realDs.AppId = appIdentity.AppId;
            }

            if (source != null) (thisDs as IDataTarget)?.Attach(source);

            lookUp = lookUp ?? source?.Configuration?.LookUpEngine;
            if (lookUp != null && thisDs.Configuration is DataSourceConfiguration dsConfig)
                dsConfig.LookUpEngine = lookUp;
            return thisDs;
        }
    }
}
