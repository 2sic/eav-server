using ToSic.Eav;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;

namespace ToSic.Testing.Shared
{
    public static class DataSourceFactoryTestExtensions
    {
        public static TDataSource TestCreate<TDataSource>(
            this IDataSourceFactory dsf,
            string noParamOrder = Parameters.Protector,
            IDataSource upstream = default,
            IAppIdentity appIdentity = default,
            ILookUpEngine configLookUp = default) where TDataSource : IDataSource
            => dsf.Create2<TDataSource>(source: upstream, configuration: new DataSourceOptions(appIdentity: appIdentity, lookUp: configLookUp));

    }
}
