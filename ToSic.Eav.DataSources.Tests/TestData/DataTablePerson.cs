using ToSic.Eav.Code;
using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.DataSourceTests.TestData;

public class DataTablePerson(ICanGetService parent)
{
    private DataSourcesTstBuilder DsSvc => field ??= parent.GetService<DataSourcesTstBuilder>();


    private DataBuilder DataBuilder => field ??= parent.GetService<DataBuilder>();

    private static readonly Dictionary<int, DataTable> CachedDs = new();

    public DataTable Generate(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
    {
        if (useCacheForSpeed && CachedDs.TryGetValue(itemsToGenerate, out var generate))
            return generate;

        var dataTable = new System.Data.DataTable();
        dataTable.Columns.AddRange([
            new(Attributes.EntityFieldId, typeof (int)),
            new(PersonSpecs.FieldFullName),
            new(PersonSpecs.FieldFirstName),
            new(PersonSpecs.FieldLastName),
            new(PersonSpecs.FieldCity),
            new(PersonSpecs.FieldIsMale, typeof (bool)),
            new(PersonSpecs.FieldBirthday, typeof (DateTime)),
            new(PersonSpecs.FieldBirthdayNull, typeof(DateTime)),
            new(PersonSpecs.FieldHeight, typeof (int)),
            new(PersonSpecs.FieldCityMaybeNull, typeof(string)),
            new(PersonSpecs.FieldModifiedInternal, typeof(DateTime))
        ]);

        new PersonGenerator(DataBuilder).GetSemiRandomList(itemsToGenerate: itemsToGenerate, firstId: firstId)
            .ForEach(person => dataTable.Rows.Add(person.Id,
                person.FullName,
                person.First,
                person.Last,
                person.City,
                person.IsMale,
                person.Birthday,
                person.BirthdayOrNull,
                person.Height,
                person.CityOrNull,
                person.Modified));

        var source = DsSvc.CreateDataSource<DataTable>(new LookUpTestData(DataBuilder).AppSetAndRes())
                .Setup(dataTable, PersonSpecs.PersonTypeName, 
                    titleField: PersonSpecs.FieldFullName, 
                    modifiedField: PersonSpecs.FieldModifiedInternal)
            //.Init(LookUpTestData.AppSetAndRes())
            ;

        // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        source.ListTac().LastOrDefault();

        if (useCacheForSpeed)
            CachedDs.Add(itemsToGenerate, source);
        return source;
    }


}