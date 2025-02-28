using ToSic.Eav.Code;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests;

public class ValueFilterMaker(ICanGetService parent)
{
    private DataSourcesTstBuilder DsSvc => field ??= parent.GetService<DataSourcesTstBuilder>();

    public ValueFilter CreateValueFilterForTesting(int itemsToGenerate, bool useDataTable, bool multiLanguage = false)
    {
        var ds = useDataTable
            ? new DataTablePerson(parent).Generate(itemsToGenerate) as IDataSource
            : DsSvc.CreateDataSource<PersonsDataSource>(new LookUpTestData(parent.GetService<DataBuilder>()).AppSetAndRes())
                .Init(itemsToGenerate, multiLanguage: multiLanguage);
        var filtered = DsSvc.CreateDataSource<ValueFilter>(ds);
        return filtered;
    }


    public ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate, bool useDataTable = true, bool multiLanguage = false)
    {
        var ds = useDataTable
            ? new DataTablePerson(parent).Generate(itemsToGenerate) as IDataSource
            : DsSvc.CreateDataSource<PersonsDataSource>(new LookUpTestData(parent.GetService<DataBuilder>()).AppSetAndRes())
                .Init(itemsToGenerate, multiLanguage: multiLanguage);
        var filtered = DsSvc.DataSourceSvc.TestCreate<ValueSort>(upstream: ds);
        return filtered;
    }


}