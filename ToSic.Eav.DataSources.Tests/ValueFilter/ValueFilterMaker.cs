using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Eav.DataSources.Configuration;

namespace ToSic.Eav.DataSourceTests
{
    public class ValueFilterMaker
    {
        private readonly DataSourceFactory _dataSourceFactory;
        public ValueFilterMaker(DataSourceFactory dataSourceFactory)
        {
            _dataSourceFactory = dataSourceFactory;
        }

        public ValueFilter CreateValueFilterForTesting(int testItemsInRootSource, bool useDataTable = true)
        {
            var ds = useDataTable
                ? DataTablePerson.Generate(testItemsInRootSource, 1001) as IDataSource
                : new PersonsDataSource().Init(testItemsInRootSource).Init(LookUpTestData.AppSetAndRes());
            var filtered = _dataSourceFactory.GetDataSource<ValueFilter>(new AppIdentity(1, 1), ds);
            return filtered;
        }

    }
}
