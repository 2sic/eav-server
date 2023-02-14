using System;
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
    public abstract partial class DataSource : ServiceBase<DataSource.Dependencies>, IDataSource, IDataTarget
    {
        /// <summary>
        /// Constructor - must be without parameters, otherwise the DI can't construct it.
        /// </summary>
        protected DataSource(Dependencies dependencies, string logName) : base(dependencies, logName)
        {
            // Load all config masks which are defined on attributes
            var configMasks = Deps.ConfigDataLoader.GetTokens(GetType());
            configMasks.ForEach(cm => ConfigMask(cm.Key, cm.Token, cm.CacheRelevant));
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        [PrivateApi("experimental")]
        public string Label { get; set; }

        /// <inheritdoc />
        public virtual int AppId { get; set; }

        /// <inheritdoc />
        public virtual int ZoneId { get; set; }

        /// <inheritdoc />
        public Guid Guid { get; set; }


        #region Properties which the Factory must add

        [PrivateApi]
        public DataSourceErrorHandling ErrorHandler => _errorHandler.Get(() => Deps.ErrorHandler.Value);
        private readonly GetOnce<DataSourceErrorHandling> _errorHandler = new GetOnce<DataSourceErrorHandling>();

        #endregion

    }
}