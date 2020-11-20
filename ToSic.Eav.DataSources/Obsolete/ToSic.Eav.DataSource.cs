// completely obsolete
// should never be used outside of DNN DLLs
#if !NETSTANDARD
using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav
{
	/// <summary>
	/// System to prepare data sources according to our needs.
	/// </summary>
	[Obsolete]
	public class DataSource: HasLog
	{
        private readonly DataSourceFactory _dsFactory;

        [Obsolete("Use DataSourceFactory with proper DI, this is only left over for compatibility in case it's used will remove in v12")]
        public DataSource(ILog parentLog = null) : base($"{DataSourceConstants.LogPrefix}.OldFct", parentLog)
        {
            _dsFactory = Factory.GetServiceProvider().Build<DataSourceFactory>().Init(Log);
        }

        [Obsolete("Use DataSourceFactory with proper DI")]
        public IDataSource GetDataSource(string sourceName, IAppIdentity app, IDataSource upstream = null,
            ILookUpEngine configLookUp = null)
            => _dsFactory.GetDataSource(sourceName, app, upstream, configLookUp);

        [Obsolete("Use DataSourceFactory with proper DI")]
        public T GetDataSource<T>(IDataSource upstream) where T : IDataSource
            => _dsFactory.GetDataSource<T>(upstream);


        [Obsolete("Use DataSourceFactory with proper DI")]
        public T GetDataSource<T>(IAppIdentity appIdentity, IDataSource upstream, ILookUpEngine configLookUp = null)
            where T : IDataSource
            => _dsFactory.GetDataSource<T>(appIdentity, upstream, configLookUp);


        [Obsolete("Use DataSourceFactory with proper DI")]
        public IDataSource GetPublishing(
            IAppIdentity app,
            bool showDrafts = false,
            ILookUpEngine configProvider = null)
            => _dsFactory.GetPublishing(app, showDrafts, configProvider);
    }

}
#endif