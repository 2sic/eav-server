using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources.ValueFilterTests;

public class ValueFilterMaker(DataSourcesTstBuilder dsSvc, Generator<DataTablePerson> personTableGenerator, DataAssembler dataAssembler, LookUpTestData lookUpTestData)
{
    public ValueFilter CreateValueFilterForTesting(int itemsToGenerate, bool useDataTable, bool multiLanguage = false)
    {
        var ds = useDataTable
            ? personTableGenerator.New().Generate(itemsToGenerate) as IDataSource
            : dsSvc.CreateDataSource<PersonsDataSource>(lookUpTestData.AppSetAndRes())
                .Init(itemsToGenerate, multiLanguage: multiLanguage);
        var filtered = dsSvc.CreateDataSource<ValueFilter>(ds);
        return filtered;
    }


    public ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate, bool useDataTable = true, bool multiLanguage = false)
    {
        var ds = useDataTable
            ? personTableGenerator.New().Generate(itemsToGenerate) as IDataSource
            : dsSvc.CreateDataSource<PersonsDataSource>(lookUpTestData.AppSetAndRes())
                .Init(itemsToGenerate, multiLanguage: multiLanguage);
        var filtered = dsSvc.DataSourceSvc.CreateTac<ValueSort>(upstream: ds);
        return filtered;
    }


}