using ToSic.Eav.Code;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.TestData;

public class DataTablePerson(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder)
{

    public DataTablePerson(ICanGetService parent): this(parent.GetService<DataSourcesTstBuilder>(), parent.GetService<DataBuilder>())
    {
    }

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

        new PersonGenerator(dataBuilder).GetSemiRandomList(itemsToGenerate: itemsToGenerate, firstId: firstId)
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

        var source = dsSvc.CreateDataSource<DataTable>(new LookUpTestData(dataBuilder).AppSetAndRes())
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