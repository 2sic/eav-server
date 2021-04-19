using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.DataSourceTests.TestData
{
    public class DataTablePerson
    {
        private static readonly Dictionary<int, DataTable> _cachedDs = new Dictionary<int, DataTable>();

        public static DataTable Generate(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            if (useCacheForSpeed && _cachedDs.ContainsKey(itemsToGenerate))
                return _cachedDs[itemsToGenerate];

            var dataTable = new System.Data.DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(DataTable.EntityIdDefaultColumnName, typeof (int)),
                new DataColumn(PersonSpecs.FieldFullName),
                new DataColumn(PersonSpecs.FieldFirstName),
                new DataColumn(PersonSpecs.FieldLastName),
                new DataColumn(PersonSpecs.FieldCity),
                new DataColumn(PersonSpecs.FieldIsMale, typeof (bool)),
                new DataColumn(PersonSpecs.FieldBirthday, typeof (DateTime)),
                new DataColumn(PersonSpecs.FieldBirthdayNull, typeof(DateTime)),
                new DataColumn(PersonSpecs.FieldHeight, typeof (int)),
                new DataColumn(PersonSpecs.FieldCityMaybeNull, typeof(string)),
                new DataColumn(PersonSpecs.FieldModifiedInternal, typeof(DateTime)),
            });

            Person.GetSemiRandomList(itemsToGenerate: itemsToGenerate, firstId: firstId)
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

            var source = new DataTable(dataTable, PersonSpecs.PersonTypeName, 
                    titleField: PersonSpecs.FieldFullName, 
                    modifiedField: PersonSpecs.FieldModifiedInternal)
                .Init(LookUpTestData.AppSetAndRes());

            // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            source.ListForTests().LastOrDefault();

            if (useCacheForSpeed)
                _cachedDs.Add(itemsToGenerate, source);
            return source;
        }


    }
}
