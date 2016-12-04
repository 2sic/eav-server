using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.DataSources.Tests
{
    internal class ValueSortShared
    {
        private static Dictionary<int, DataTableDataSource> _cachedDs = new Dictionary<int, DataTableDataSource>();

        public static ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(itemsToGenerate, firstId);
            var filtered = DataSource.GetDataSource<ValueSort>(1, 1, ds);
            return filtered;

            //if (useCacheForSpeed && _cachedDs.ContainsKey(itemsToGenerate))
            //    return _cachedDs[itemsToGenerate];

            //var dataTable = new DataTable();
            //dataTable.Columns.AddRange(new[]
            //{
            //    new DataColumn(DataTableDataSource.EntityIdDefaultColumnName, typeof (int)),
            //    new DataColumn("FullName"),
            //    new DataColumn("FirstName"),
            //    new DataColumn("LastName"),
            //    new DataColumn("City"),
            //    new DataColumn("IsMale", typeof (bool)),
            //    new DataColumn("Birthdate", typeof (DateTime)),
            //    new DataColumn("Height", typeof (int)),
            //    new DataColumn("CityMaybeNull", typeof(string)),
            //});

            //AddSemirandomPersons(dataTable, itemsToGenerate, firstId);

            //var source = new DataTableDataSource(dataTable, "Person", titleField: "FullName")
            //{
            //    ConfigurationProvider = new UnitTests.ValueProvider.ValueCollectionProvider_Test().ValueCollection()
            //};

            //// now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            //var temp = source.LightList.LastOrDefault();

            //if (useCacheForSpeed)
            //    _cachedDs.Add(itemsToGenerate, source);
            //return source;
        }

    }
}
