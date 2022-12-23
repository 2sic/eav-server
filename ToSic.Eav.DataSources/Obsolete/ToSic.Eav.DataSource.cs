// 2022-12-24 2dm Completely disabled

//// completely obsolete
//// should never be used outside of DNN DLLs
//#if !NETSTANDARD
//using System;
//using ToSic.Eav.Apps;
//using ToSic.Eav.DataSources;
//using ToSic.Lib.Logging;
//using ToSic.Eav.LookUp;

//// ReSharper disable once CheckNamespace
//namespace ToSic.Eav
//{
//	/// <summary>
//	/// System to prepare data sources according to our needs.
//	/// </summary>
//	[Obsolete]
//	public class DataSource: HasLog
//	{
//        [Obsolete("Use DataSourceFactory with proper DI, this is only left over for compatibility in case it's used will remove in v12")]
//        public DataSource(ILog parentLog = null) : base($"{DataSourceConstants.LogPrefix}.OldFct", parentLog)
//        {
//            throw new Exception($"The {nameof(DataSource)} object is obsolete. Please fix using these instructions: https://r.2sxc.org/brc-13-datasource");
//        }

//        // The following methods are still included, to ensure the constructor can be called
//        // Otherwise there is the risk that a compiler breaks before the constructor is called.

//        [Obsolete("Use DataSourceFactory with proper DI")]
//        public IDataSource GetDataSource(string sourceName, IAppIdentity app, IDataSource upstream = null,
//            ILookUpEngine configLookUp = null) => null;

//        [Obsolete("Use DataSourceFactory with proper DI")]
//        public T GetDataSource<T>(IDataSource upstream) where T : IDataSource
//            => throw new NotSupportedException();


//        [Obsolete("Use DataSourceFactory with proper DI")]
//        public T GetDataSource<T>(IAppIdentity appIdentity, IDataSource upstream, ILookUpEngine configLookUp = null)
//            where T : IDataSource
//            => throw new NotSupportedException();


//        [Obsolete("Use DataSourceFactory with proper DI")]
//        public IDataSource GetPublishing(
//            IAppIdentity app,
//            bool showDrafts = false,
//            ILookUpEngine configProvider = null)
//            => throw new NotSupportedException();
//    }

//}
//#endif