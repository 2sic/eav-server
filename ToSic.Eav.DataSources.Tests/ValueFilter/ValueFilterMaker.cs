using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    public class ValueFilterMaker: TestServiceBase
    {
        private readonly DataSourceFactory _dataSourceFactory;
        public ValueFilterMaker(IServiceBuilder parent): base(parent)
        {
            _dataSourceFactory = Build<DataSourceFactory>();
        }

        public ValueFilter CreateValueFilterForTesting(int itemsToGenerate, bool useDataTable, bool multiLanguage = false)
        {
            var ds = useDataTable
                ? new DataTablePerson(this).Generate(itemsToGenerate) as IDataSource
                : _dataSourceFactory.GetDataSource<PersonsDataSource>(new AppIdentity(1, 1), null, new LookUpEngine())
                    .Init(itemsToGenerate, multiLanguage: multiLanguage).Init(LookUpTestData.AppSetAndRes());
            var filtered = _dataSourceFactory.GetDataSource<ValueFilter>(new AppIdentity(1, 1), ds);
            return filtered;
        }


        public ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate, bool useDataTable = true, bool multiLanguage = false)
        {
            var ds = useDataTable
                ? new DataTablePerson(this).Generate(itemsToGenerate) as IDataSource
                : _dataSourceFactory.GetDataSource<PersonsDataSource>(new AppIdentity(1, 1), null, new LookUpEngine())
                    .Init(itemsToGenerate, multiLanguage: multiLanguage).Init(LookUpTestData.AppSetAndRes());
            var filtered = _dataSourceFactory.GetDataSource<ValueSort>(ds, ds);
            return filtered;
        }


    }
}
