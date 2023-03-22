using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public class DataSourceGenerator<TDataSource>: ServiceBase, IDataSourceGenerator<TDataSource> where TDataSource : IDataSource
    {
        private readonly LazySvc<DataSourceFactory> _dataSourceFactory;

        public DataSourceGenerator(LazySvc<DataSourceFactory> dataSourceFactory): base("DS.DsGen")
        {
            _dataSourceFactory = dataSourceFactory;
        }

        public TDataSource New(
            string noParamOrder = Parameters.Protector,
            IDataSource source = default,
            IDataSourceConfiguration configuration = default) =>
            _dataSourceFactory.Value.Create<TDataSource>(source: source, configuration: configuration);
    }
}
