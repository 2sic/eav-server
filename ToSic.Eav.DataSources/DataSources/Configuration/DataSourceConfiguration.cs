using System;
using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public class DataSourceConfiguration : ServiceBase, IDataSourceConfiguration
    {
        #region Dependencies - Must be in DI

        public class Dependencies: ServiceDependencies
        {
            public LazySvc<IZoneCultureResolver> ZoneCultureResolverLazy { get; }

            public Dependencies(LazySvc<IZoneCultureResolver> zoneCultureResolverLazy)
            {
                AddToLogQueue(
                    ZoneCultureResolverLazy = zoneCultureResolverLazy
                );
            }
        }

        #endregion

        #region Constructor (non DI)

        [PrivateApi] public DataSourceConfiguration(Dependencies dependencies, DataSource ds) : base($"{DataSourceConstants.LogPrefix}.Config")
        {
            _deps = dependencies.SetLog(Log);
            DataSource = ds;
        }

        private readonly Dependencies _deps;
        [PrivateApi] internal DataSource DataSource;

        #endregion

        public string this[string key]
        {
            get => Values[key];
            set => Values[key] = value;
        }

        [PrivateApi("just included for compatibility, as previous public examples used Add")]
        [Obsolete("please use the indexer instead - Configuration[key] = value")]
        public void Add(string key, string value) => this[key] = value;

        [PrivateApi]
        public IDictionary<string, string> Values { get; internal set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);


        public ILookUpEngine LookUpEngine { get; protected internal set; }

        [PrivateApi]
        public bool IsParsed { get; private set; }

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
            if (LookUpEngine == null)
                throw new Exception($"No ConfigurationProvider configured on this data-source. Cannot run {nameof(Parse)}");

            // construct a property access for in, use it in the config provider
            return LookUpEngine.LookUp(values, OverrideLookUps);
        }

        /// <summary>
        /// An internally created lookup to give access to the In-streams if there are any
        /// </summary>
        [PrivateApi]
        private IDictionary<string, ILookUp> OverrideLookUps 
            => _overrideLookUps 
               ?? (_overrideLookUps = new Dictionary<string, ILookUp> { { "In".ToLowerInvariant(), new LookUpInDataTarget(DataSource, _deps.ZoneCultureResolverLazy.Value) } });
        private IDictionary<string, ILookUp> _overrideLookUps;


        [PrivateApi]
        public static bool TryConvertToBool(string value, bool? defaultValue = null)
        {
            var defValue = defaultValue ?? false;
            if (string.IsNullOrWhiteSpace(value)) return defValue;
            if (bool.TryParse(value, out var result)) return result;
            return defValue;
        }
    }
}
