using System;
using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources.Configuration
{
    public class DataSourceConfiguration
    {
        public DataSourceBase DataSource;
        
        public DataSourceConfiguration(DataSourceBase ds)
        {
            DataSource = ds;
        }

        // todo: rename to values
        public IDictionary<string, string> Configuration { get; internal set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // todo: rename to lookup
        public ILookUpEngine ConfigurationProvider { get; protected internal set; }

        protected internal bool ConfigurationIsLoaded;

        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
        [PrivateApi]
        protected internal virtual void EnsureConfigurationIsLoaded()
        {
            if (ConfigurationIsLoaded)
                return;

            // Ensure that we have a configuration-provider (not always the case, but required)
            if (ConfigurationProvider == null)
                throw new Exception($"No ConfigurationProvider configured on this data-source. Cannot run {nameof(EnsureConfigurationIsLoaded)}");

            // construct a property access for in, use it in the config provider
            var instancePAs = new Dictionary<string, ILookUp> { { "In".ToLower(), new LookUpInDataTarget(DataSource) } };
            Configuration = ConfigurationProvider.LookUp(Configuration, instancePAs);
            ConfigurationIsLoaded = true;
        }

    }
}
