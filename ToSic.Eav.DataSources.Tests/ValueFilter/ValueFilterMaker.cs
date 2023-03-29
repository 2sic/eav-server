using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
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
                : Parent.CreateDataSource<PersonsDataSource>(new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes())
                    .Init(itemsToGenerate, multiLanguage: multiLanguage);
            var filtered = Parent.CreateDataSource<ValueFilter>(ds);
            return filtered;
        }


        public ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate, bool useDataTable = true, bool multiLanguage = false)
        {
            var ds = useDataTable
                ? new DataTablePerson(Parent).Generate(itemsToGenerate) as IDataSource
                : Parent.CreateDataSource<PersonsDataSource>(new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes())
                    .Init(itemsToGenerate, multiLanguage: multiLanguage);
            var filtered = Parent.DataSourceFactory.TestCreate<ValueSort>(upstream: ds);
            return filtered;
        }


    }
}
