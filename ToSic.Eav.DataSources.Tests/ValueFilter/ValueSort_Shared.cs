using System.Collections.Generic;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.DataSources.Tests
{
    internal class ValueSortShared
    {
        private static Dictionary<int, DataTable> _cachedDs = new Dictionary<int, DataTable>();

        public static ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(itemsToGenerate, firstId);
            var filtered = Eav.DataSource.GetDataSource<ValueSort>(ds, null);
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
