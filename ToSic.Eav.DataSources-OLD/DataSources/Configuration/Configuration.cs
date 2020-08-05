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

        public IDictionary<string, string> Values { get; internal set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ILookUpEngine LookUps { get; protected internal set; }

        protected internal bool IsParsed;

        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
        [PrivateApi]
        public void Parse()
        {
            if (IsParsed) return;
            Values = Parse(Values);
            IsParsed = true;
        }

        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
        [PrivateApi]
        public IDictionary<string, string> Parse(IDictionary<string, string> values)
        {
            // Ensure that we have a configuration-provider (not always the case, but required)
            if (LookUps == null)
                throw new Exception($"No ConfigurationProvider configured on this data-source. Cannot run {nameof(Parse)}");

            // construct a property access for in, use it in the config provider
            return LookUps.LookUp(values, OverrideLookUps);
        }

        [PrivateApi]
        internal IDictionary<string, ILookUp> OverrideLookUps 
            => _overrideLookUps 
               ?? (_overrideLookUps = new Dictionary<string, ILookUp> { { "In".ToLower(), new LookUpInDataTarget(DataSource) } });
        private IDictionary<string, ILookUp> _overrideLookUps;


        [PrivateApi]
        public bool ConvertSafely(string value, bool? defaultValue = null)
        {
            var defValue = defaultValue ?? false;
            if (string.IsNullOrWhiteSpace(value)) return defValue;
            if (bool.TryParse(value, out var result))
                return result;
            return defValue;
        }
    }
}
