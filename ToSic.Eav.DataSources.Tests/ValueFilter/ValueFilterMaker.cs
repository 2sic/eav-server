using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.ValueFilter
{
    public class ValueFilterMaker
    {
        private readonly DataSourceFactory _dataSourceFactory;
        public ValueFilterMaker(DataSourceFactory dataSourceFactory)
        {
            _dataSourceFactory = dataSourceFactory;
        }

        public DataSources.ValueFilter CreateValueFilterForTesting(int testItemsInRootSource)
        {
            var ds = DataTableTst.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = _dataSourceFactory.GetDataSource<DataSources.ValueFilter>(new AppIdentity(1, 1), ds);
            return filtered;
        }

    }
}
