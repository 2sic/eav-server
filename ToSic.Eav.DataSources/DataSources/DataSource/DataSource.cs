using System;
using ToSic.Eav.Apps;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The base class for all DataSources, especially custom DataSources.
    /// It must always be inherited.
    /// It provides a lot of core functionality to get configurations, ensure caching and more.
    ///
    /// Important: in most cases you will inherit the <see cref="CustomDataSourceAdvanced"/> DataSource for custom data sources.
    /// </summary>
    /// <remarks>
    /// Had a major, breaking update in v15.
    /// Consult the guide to upgrade your custom data sources.
    /// </remarks>
    [PublicApi]
    public abstract partial class DataSource : ServiceBase<DataSource.MyServices>, IDataSource, IAppIdentitySync
    {

        /// <summary>
        /// Default Constructor, _protected_.
        /// To inherit this, make sure your new class also gets the `MyServices` in it's constructor and passes it to here.
        /// </summary>
        /// <param name="services">All the services a DataSource needs. Uses the MyServices convention TODO: DOCUMENT</param>
        /// <param name="logName">Your own log name, such as `My.CsvDs`</param>
        protected DataSource(MyServices services, string logName) : base(services, logName)
        {
            AutoLoadAllConfigMasks(GetType());
        }
        protected DataSource(MyServicesBase<MyServices> extendedServices, string logName) : base(extendedServices, logName)
        {
            AutoLoadAllConfigMasks(GetType());
        }

        /// <summary>
        /// Load all [Configuration] attributes and ensure we have the config masks.
        /// </summary>
        [PrivateApi]
        internal void AutoLoadAllConfigMasks(Type type)
        {
            // Load all config masks which are defined on attributes
            var configMasks = Services.ConfigDataLoader.GetTokens(type);
            configMasks.ForEach(cm => ConfigMask(cm.Key, cm.Token, cm.CacheRelevant));
        }


        /// <inheritdoc />
        public string Name => GetType().Name;

        [PrivateApi("internal use only - for labeling data sources in queries to show in debugging")]
        public string Label { get; set; }

        /// <inheritdoc />
        public virtual int AppId { get; protected set; }

        /// <inheritdoc />
        public virtual int ZoneId { get; protected set; }

        /// <inheritdoc />
        public Guid Guid { get; internal set; }


        #region Error Handling

        [PublicApi]
        public DataSourceErrorHelper Error => _errorHandler.Get(() => Services.ErrorHandler.Value.Link(this));
        private readonly GetOnce<DataSourceErrorHelper> _errorHandler = new GetOnce<DataSourceErrorHelper>();

        #endregion

        void IAppIdentitySync.UpdateAppIdentity(IAppIdentity appIdentity)
        {
            AppId = appIdentity.AppId;
            ZoneId = appIdentity.ZoneId;
        }
    }
}