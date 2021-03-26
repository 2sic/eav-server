using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The base class, which should always be inherited. Already implements things like Get One / Get many, Caching and a lot more.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract partial class DataSourceBase : HasLog, IDataSource, IDataTarget
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







        #region Special Region so that each data sources has a factory if needed

        [PrivateApi]
        protected internal DataSourceFactory DataSourceFactory { get; set; }

        #endregion

    }
}