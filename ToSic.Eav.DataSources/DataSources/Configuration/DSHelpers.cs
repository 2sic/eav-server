﻿using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Linking;
using ToSic.Eav.Generics;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
    internal static class DSHelpers
    {
        public static T Init<T>(this T thisDs, ILookUpEngine lookUpEngine) where T : IDataSource
        {
            if (lookUpEngine != null && thisDs.Configuration is DataSourceConfiguration dsConfig)
                dsConfig.LookUpEngine = lookUpEngine;
            return thisDs;
        }

        /// <summary>
        /// Helper function (internal) to configure a new data source.
        /// </summary>
        /// <param name="newDataSource">The new data source</param>
        /// <param name="source">upstream data source - for auto-attaching</param>
        /// <param name="configuration">optional configuration provider</param>
        public static T Init<T>(this T newDataSource, /*IDataSource source,*/ IDataSourceOptions configuration, IDataSourceLink links) where T : IDataSource
        {
            if (newDataSource == null) throw new ArgumentNullException(nameof(newDataSource));

            var mainUpstream = links?.Link?.DataSource;
            (newDataSource as IAppIdentitySync)?.UpdateAppIdentity(configuration?.AppIdentity ?? mainUpstream);

            //if (source != null) newDataSource.Attach(source);
            if (links?.Link != null) newDataSource.Connect(links.Link);

            var lookUp = configuration?.LookUp ?? mainUpstream?.Configuration?.LookUpEngine;
            if (lookUp != null && newDataSource.Configuration is DataSourceConfiguration dsConfig)
            {
                dsConfig.LookUpEngine = lookUp;
                var configValues = configuration?.Values;
                if (configValues != null) dsConfig.AddMany(configValues.ToEditable());
            }
            return newDataSource;
        }
    }
}
