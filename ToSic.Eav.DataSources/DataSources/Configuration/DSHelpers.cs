using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
    internal static class DSHelpers
    {
        public static T Init<T>(this T thisDs, ILookUpEngine configSource) where T : IDataSource
        {
            if (configSource != null && thisDs.Configuration is DataSourceConfiguration dsConfig)
                dsConfig.LookUpEngine = configSource;
            return thisDs;
        }

        /// <summary>
        /// Helper function (internal) to configure a new data source.
        /// </summary>
        /// <param name="thisDs">The new data source</param>
        /// <param name="appIdentity">app identifier</param>
        /// <param name="source">upstream data source - for auto-attaching</param>
        /// <param name="configSource">optional configuration provider - for auto-attaching</param>
        public static T Init<T>(this T thisDs,
            IAppIdentity appIdentity,
            IDataSource source = default,
            ILookUpEngine configSource = default,
            IDictionary<string, string> configuration = default) where T : IDataSource
        {
            if (thisDs == null) throw new ArgumentNullException(nameof(thisDs));

            (thisDs as IAppIdentitySync)?.UpdateAppIdentity(appIdentity ?? source);

            if (source != null) thisDs.Attach(source);

            configSource = configSource ?? source?.Configuration?.LookUpEngine;
            if (configSource != null && thisDs.Configuration is DataSourceConfiguration dsConfig)
            {
                dsConfig.LookUpEngine = configSource;
                if (configuration != null) dsConfig.AddMany(configuration);
            }
            return thisDs;
        }
    }
}
