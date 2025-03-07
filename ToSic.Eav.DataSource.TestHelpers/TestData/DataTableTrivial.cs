using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.TestData;

public class DataTableTrivial(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder)
{
    //private DataSourcesTstBuilder DsSvc => field ??= parent.GetService<DataSourcesTstBuilder>();

    private static readonly Dictionary<int, DataTable> CachedDs = new();

    public DataTable Generate(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
    {
        var dataTable = new System.Data.DataTable();
        dataTable.Columns.AddRange([
            new(Attributes.EntityFieldId, typeof (int)),
            new("EntityTitle"),
            new(PersonSpecs.FieldFirstName),
            new(PersonSpecs.FieldLastName),
            new(PersonSpecs.FieldCity),
            new(PersonSpecs.FieldModifiedInternal, typeof(DateTime))
        ]);
        AddSemiRandomTrivial(dataTable, itemsToGenerate, firstId);

        var source = dsSvc
                .CreateDataSource<DataTable>(new LookUpTestData(dataBuilder).AppSetAndRes())
                .Setup(dataTable, "Person", modifiedField: PersonSpecs.FieldModifiedInternal)
            ;

        // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        source.List.LastOrDefault();

        if (useCacheForSpeed)
            CachedDs.Add(itemsToGenerate, source);
        return source;
    }

    private static void AddSemiRandomTrivial(System.Data.DataTable dataTable, int itemsToGenerate = 10, int firstId = 1000)
    {
        for (var i = firstId; i < firstId + itemsToGenerate; i++)
        {
            var firstName = "Daniel";
            var lastName = "Mettler";
            var fullName = firstName + " " + lastName;
            var city = PersonSpecs.TestCities[i % PersonSpecs.TestCities.Length];
            var sysModified = RandomData.RandomDate();
            dataTable.Rows.Add(i,
                fullName,
                firstName,
                lastName,
                city,
                sysModified
            );
        }
    }

}