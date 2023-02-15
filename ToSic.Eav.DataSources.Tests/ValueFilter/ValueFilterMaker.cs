using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    public class ValueFilterMaker: TestServiceBase
    {
        public ValueFilterMaker(TestBaseEavDataSource parent) : base(parent)
        {
        }

        public ValueFilter CreateValueFilterForTesting(int itemsToGenerate, bool useDataTable, bool multiLanguage = false)
        {
            var ds = useDataTable
                ? new DataTablePerson(Parent).Generate(itemsToGenerate) as IDataSource
                : Parent.CreateDataSource<PersonsDataSource>(LookUpTestData.AppSetAndRes())
                    .Init(itemsToGenerate, multiLanguage: multiLanguage);
            var filtered = Parent.CreateDataSource<ValueFilter>(ds);
            return filtered;
        }


        public ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate, bool useDataTable = true, bool multiLanguage = false)
        {
            var ds = useDataTable
                ? new DataTablePerson(Parent).Generate(itemsToGenerate) as IDataSource
                : Parent.CreateDataSource<PersonsDataSource>(LookUpTestData.AppSetAndRes())
                    .Init(itemsToGenerate, multiLanguage: multiLanguage);
            var filtered = Parent.DataSourceFactory.GetDataSource<ValueSort>(ds);
            return filtered;
        }


    }
}
