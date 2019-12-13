using System;
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

        public string this[string key]
        {
            get => Values[key];
            set => Values[key] = value;
        }

        // todo: rename to values
        public IDictionary<string, string> Values { get; internal set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // todo: rename to lookup
        public ILookUpEngine LookUps { get; protected internal set; }

        protected internal bool IsParsed;

        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
        [PrivateApi]
        public void Parse()
        {
            if (IsParsed)
                return;

            // Ensure that we have a configuration-provider (not always the case, but required)
            if (LookUps == null)
                throw new Exception($"No ConfigurationProvider configured on this data-source. Cannot run {nameof(Parse)}");

            // construct a property access for in, use it in the config provider
            var instancePAs = new Dictionary<string, ILookUp> { { "In".ToLower(), new LookUpInDataTarget(DataSource) } };
            Values = LookUps.LookUp(Values, instancePAs);
            IsParsed = true;
        }

    }
}
