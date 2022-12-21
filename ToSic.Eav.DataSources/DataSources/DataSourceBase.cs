using System;
using ToSic.Eav.Data;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helper;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The base class, which should always be inherited. Already implements things like Get One / Get many, Caching and a lot more.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract partial class DataSourceBase : ServiceBase, IDataSource, IDataTarget
    {
        /// <inheritdoc/>
        [PrivateApi]
        public abstract string LogId { get; }

        /// <summary>
        /// Constructor - must be without parameters, otherwise the DI can't construct it.
        /// </summary>
        protected DataSourceBase() : base("DS.Base") { }

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
        public IDataSourceConfiguration Configuration => _config ?? (_config = new DataSourceConfiguration(this));
        private IDataSourceConfiguration _config;




        #region Properties which the Factory must add

        protected IDataBuilder DataBuilder => _dataBuilder.Get(() => _dataBuilderLazy.New());
        private readonly GetOnce<IDataBuilder> _dataBuilder = new GetOnce<IDataBuilder>();
        [PrivateApi]
        internal Generator<IDataBuilder> _dataBuilderLazy;

        [PrivateApi] public DataSourceErrorHandling ErrorHandler => _dataSourceErrorHandlingLazy.Value;
        [PrivateApi] internal LazySvc<DataSourceErrorHandling> _dataSourceErrorHandlingLazy;

        #endregion


        #region Special Region so that each data sources has a factory if needed

        [PrivateApi]
        protected internal DataSourceFactory DataSourceFactory { get; set; }

        #endregion

    }
}