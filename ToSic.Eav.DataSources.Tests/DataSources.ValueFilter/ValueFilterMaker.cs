using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSourceTests;

public class ValueFilterMaker(DataSourcesTstBuilder dsSvc, Generator<DataTablePerson> personTableGenerator, DataBuilder dataBuilder)
{
    public ValueFilter CreateValueFilterForTesting(int itemsToGenerate, bool useDataTable, bool multiLanguage = false)
    {
        var ds = useDataTable
            ? personTableGenerator.New().Generate(itemsToGenerate) as IDataSource
            : dsSvc.CreateDataSource<PersonsDataSource>(new LookUpTestData(dataBuilder).AppSetAndRes())
                .Init(itemsToGenerate, multiLanguage: multiLanguage);
        var filtered = dsSvc.CreateDataSource<ValueFilter>(ds);
        return filtered;
    }


    public ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate, bool useDataTable = true, bool multiLanguage = false)
    {
        var ds = useDataTable
            ? personTableGenerator.New().Generate(itemsToGenerate) as IDataSource
            : dsSvc.CreateDataSource<PersonsDataSource>(new LookUpTestData(dataBuilder).AppSetAndRes())
                .Init(itemsToGenerate, multiLanguage: multiLanguage);
        var filtered = dsSvc.DataSourceSvc.CreateTac<ValueSort>(upstream: ds);
        return filtered;
    }


}