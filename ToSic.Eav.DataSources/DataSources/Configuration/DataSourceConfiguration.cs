using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ToSic.Eav.Context;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;
using static System.StringComparer;

namespace ToSic.Eav.DataSources
{
    public class DataSourceConfiguration : ServiceBase<DataSourceConfiguration.Dependencies>, IDataSourceConfiguration
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

        [PrivateApi]
        public DataSourceConfiguration(Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Config")
        {
        }

        internal DataSourceConfiguration Attach(DataSource ds)
        {
            DataSourceForIn = ds;
            return this;
        }


        [PrivateApi] internal DataSource DataSourceForIn;

        #endregion

        // 2022-02-14 2dm disabled, as all DataSources will need recompiling - Remove 2023 Q2

        //public string this[string key]
        //{
        //    get => Values[key];
        //    set => Values[key] = value;
        //}


        public string GetThis([CallerMemberName] string cName = default) => Values.TryGetValue(cName, out var result) 
            ? result 
            : throw new ArgumentException($"Trying to get a configuration by name of {cName} but it doesn't exist. Did you forget to add to ConfigMask?");

        public T GetThis<T>(T fallback, [CallerMemberName] string cName = default)
            => Values.TryGetValue(cName, out var result)
                ? result.ConvertOrFallback(fallback)
                : fallback;

        //public T Get<T>(string key, T fallback) => Values.TryGetValue(key, out var result)
        //    ? result.ConvertOrFallback(fallback)
        //    : fallback;

        //public void Set<T>(string key, T value) => this[key] = value.ToString();

        public void SetThis(string value, [CallerMemberName] string cName = default) => Values[cName] = value;

        public void SetThis<T>(T value, [CallerMemberName] string cName = default) => Values[cName] = value?.ToString();

        // 2022-02-14 2dm disabled, as all DataSources will need recompiling - Remove 2023 Q2
        //[PrivateApi("just included for compatibility, as previous public examples used Add")]
        //[Obsolete("please use the indexer instead - Configuration[key] = value")]
        //public void Add(string key, string value) => this[key] = value;

        [PrivateApi]
        public IDictionary<string, string> Values { get; internal set; } = new Dictionary<string, string>(InvariantCultureIgnoreCase);


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
            => _overrideLookUps.Get(() => new Dictionary<string, ILookUp>
            {
                { "In".ToLowerInvariant(), new LookUpInDataTarget(DataSourceForIn, Deps.ZoneCultureResolverLazy.Value) }
            });
        private readonly GetOnce<IDictionary<string, ILookUp>> _overrideLookUps = new GetOnce<IDictionary<string, ILookUp>>();


    }
}
