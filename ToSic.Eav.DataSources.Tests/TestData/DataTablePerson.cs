using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Testing.Shared;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.DataSourceTests.TestData
{
    public class DataTablePerson: TestServiceBase
    {
        public DataTablePerson(IServiceBuilder serviceProvider) : base(serviceProvider)
        {
            _multiBuilder = Build<MultiBuilder>();
        }

        private MultiBuilder _multiBuilder;

        private static readonly Dictionary<int, DataTable> CachedDs = new Dictionary<int, DataTable>();

        public DataTable Generate(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            if (useCacheForSpeed && CachedDs.ContainsKey(itemsToGenerate))
                return CachedDs[itemsToGenerate];

            var dataTable = new System.Data.DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(Attributes.EntityFieldId, typeof (int)),
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

            new PersonGenerator(_multiBuilder).GetSemiRandomList(itemsToGenerate: itemsToGenerate, firstId: firstId)
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

            var source = this.GetTestDataSource<DataTable>(LookUpTestData.AppSetAndRes())
                .Setup(dataTable, PersonSpecs.PersonTypeName, 
                    titleField: PersonSpecs.FieldFullName, 
                    modifiedField: PersonSpecs.FieldModifiedInternal)
                //.Init(LookUpTestData.AppSetAndRes())
                ;

            // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            source.ListForTests().LastOrDefault();

            if (useCacheForSpeed)
                CachedDs.Add(itemsToGenerate, source);
            return source;
        }


    }
}
