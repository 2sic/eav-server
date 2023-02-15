using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data;
using ToSic.Testing.Shared;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.DataSourceTests.TestData
{
    public class DataTableTrivial: TestServiceBase
    {
        private readonly TestBaseEavDataSource _parent;

        public DataTableTrivial(TestBaseEavDataSource parent) : base(parent)
        {
            _parent = parent;
        }

        private static readonly Dictionary<int, DataTable> CachedDs = new Dictionary<int, DataTable>();
        
        public DataTable Generate(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            var dataTable = new System.Data.DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(Attributes.EntityFieldId, typeof (int)),
                new DataColumn("EntityTitle"),
                new DataColumn(PersonSpecs.FieldFirstName),
                new DataColumn(PersonSpecs.FieldLastName),
                new DataColumn(PersonSpecs.FieldCity),
                new DataColumn(PersonSpecs.FieldModifiedInternal, typeof(DateTime)),
            });
            AddSemirandomTrivial(dataTable, itemsToGenerate, firstId);

            var source = _parent.CreateDataSource<DataTable>(LookUpTestData.AppSetAndRes())
                .Setup(dataTable, "Person", modifiedField: PersonSpecs.FieldModifiedInternal)
                //.Init(LookUpTestData.AppSetAndRes())
                ;

            // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            source.List.LastOrDefault();

            if (useCacheForSpeed)
                CachedDs.Add(itemsToGenerate, source);
            return source;
        }

        private static void AddSemirandomTrivial(System.Data.DataTable dataTable, int itemsToGenerate = 10, int firstId = 1000)
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
}
