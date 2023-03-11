using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The base class, which should always be inherited. Already implements things like Get One / Get many, Caching and a lot more.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract partial class DataSource : ServiceBase<DataSource.MyServices>, IDataSource, IDataTarget
    {
        /// <summary>
        /// Constant empty list of entities - for common scenarios where we just need to return no hits.
        /// </summary>
        public static IImmutableList<IEntity> EmptyList = ImmutableList<IEntity>.Empty;

        /// <summary>
        /// Constructor - must be without parameters, otherwise the DI can't construct it.
        /// </summary>
        protected DataSource(MyServices services, string logName) : base(services, logName)
        {
            AutoLoadAllConfigMasks();
        }
        protected DataSource(MyServicesBase<MyServices> extendedServices, string logName) : base(extendedServices, logName)
        {
            AutoLoadAllConfigMasks();
        }

        /// <summary>
        /// Load all [Configuration] attributes and ensure we have the config masks.
        /// </summary>
        private void AutoLoadAllConfigMasks()
        {
            // Load all config masks which are defined on attributes
            var configMasks = Services.ConfigDataLoader.GetTokens(GetType());
            configMasks.ForEach(cm => ConfigMask(cm.Key, cm.Token, cm.CacheRelevant));
        }


        /// <inheritdoc />
        public string Name => GetType().Name;

        [PrivateApi("experimental")]
        public string Label { get; set; }

        /// <inheritdoc />
        public virtual int AppId { get; protected internal set; }

        /// <inheritdoc />
        public virtual int ZoneId { get; protected internal set; }

        /// <inheritdoc />
        public Guid Guid { get; set; }


        #region Error Handling

        [PublicApi]
        public DataSourceErrorHelper Error => _errorHandler.Get(() => Services.ErrorHandler.Value);
        private readonly GetOnce<DataSourceErrorHelper> _errorHandler = new GetOnce<DataSourceErrorHelper>();

        #endregion

    }
}