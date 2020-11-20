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

        public ValueFilter CreateValueFilterForTesting(int itemsToGenerate, bool useDataTable, bool multiLanguage = false)
        {
            var ds = useDataTable
                ? DataTablePerson.Generate(itemsToGenerate) as IDataSource
                : new PersonsDataSource().Init(itemsToGenerate, multiLanguage: multiLanguage).Init(LookUpTestData.AppSetAndRes());
            var filtered = _dataSourceFactory.GetDataSource<ValueFilter>(new AppIdentity(1, 1), ds);
            return filtered;
        }


        public ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate, bool useDataTable = true, bool multiLanguage = false)
        {
            var ds = useDataTable
                ? DataTablePerson.Generate(itemsToGenerate) as IDataSource
                : new PersonsDataSource().Init(itemsToGenerate, multiLanguage: multiLanguage).Init(LookUpTestData.AppSetAndRes());
            var filtered = _dataSourceFactory.GetDataSource<ValueSort>(ds, ds);
            return filtered;
        }


    }
}
