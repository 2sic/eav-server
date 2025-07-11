﻿using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.LookUp;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.TestData;

public class DataTablePerson(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder)
{

    private static readonly Dictionary<int, DataTable> CachedDs = new();

    public DataTable Generate(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
    {
        if (useCacheForSpeed && CachedDs.TryGetValue(itemsToGenerate, out var generate))
            return generate;

        var dataTable = new System.Data.DataTable();
        dataTable.Columns.AddRange([
            new(AttributeNames.EntityFieldId, typeof (int)),
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