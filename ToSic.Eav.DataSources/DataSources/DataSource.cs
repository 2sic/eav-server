using System;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helper;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The base class, which should always be inherited. Already implements things like Get One / Get many, Caching and a lot more.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract partial class DataSource : ServiceBase, IDataSource, IDataTarget
    {
        /// <summary>
        /// Constructor - must be without parameters, otherwise the DI can't construct it.
        /// </summary>
        protected DataSource(Dependencies dependencies, string logName) : base(logName)
        {
            _deps = dependencies.SetLog(Log);
        }
        private readonly Dependencies _deps;

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


        /// <inheritdoc />
        public IDataSourceConfiguration Configuration => _config ?? (_config = new DataSourceConfiguration(_deps.ConfigDependencies, this));
        private IDataSourceConfiguration _config;




        #region Properties which the Factory must add

        protected IDataBuilder DataBuilder => _dataBuilder.Get(() => _deps.DataBuilder.Value);
        private readonly GetOnce<IDataBuilder> _dataBuilder = new GetOnce<IDataBuilder>();

        [PrivateApi]
        public DataSourceErrorHandling ErrorHandler => _errorHandler.Get(() => _deps.ErrorHandler.Value);
        private readonly GetOnce<DataSourceErrorHandling> _errorHandler = new GetOnce<DataSourceErrorHandling>();

        #endregion

    }
}