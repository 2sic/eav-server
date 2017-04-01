using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class DataTableDataSourceTest
    {
        public static string[] TestCities = { "Buchs", "Grabs", "Sevelen", "Zürich" };
        public static int MinHeight = 150;
        public static int HeightVar = 55;
        public static int IsMaleForEveryX = 3;

        private static readonly Dictionary<int, DataTableDataSource> _cachedDs = new Dictionary<int, DataTableDataSource>();

        [TestMethod]
        public void DataSource_Create_GeneralTest()
        {
            const int itemsToGenerate = 499;
            var ds = GeneratePersonSourceWithDemoData(itemsToGenerate);
            Assert.IsTrue(ds.In.Count == 0, "In count should be 0");
            Assert.IsTrue(ds.Out.Count == 1, "Out cound should be 1");
            var defaultOut = ds["Default"];
            Assert.IsTrue(defaultOut != null);
            try
            {
                // ReSharper disable once UnusedVariable
                var x = ds["Something"];
                Assert.Fail("Access to another out should fail");
            }
            catch { }
            Assert.IsTrue(defaultOut.List.Count == itemsToGenerate);
        }

        [TestMethod]
        public void DataTable_CacheKey()
        {
            const int itemsToGenerate = 499;
            var ds = GeneratePersonSourceWithDemoData(itemsToGenerate);

            Assert.AreEqual("DataTableDataSource-NoGuid&ContentType=Person", ds.CachePartialKey);
            Assert.AreEqual("DataTableDataSource-NoGuid&ContentType=Person", ds.CacheFullKey);
            var lastRefresh = ds.CacheLastRefresh; // get this before comparison, because sometimes slow execution will get strange results
            Assert.IsTrue(DateTime.Now >= lastRefresh, "Date-check of cache refresh");
        }

        [TestMethod]
        public void DataTable_DefaultTitleField()
        {
            const int itemsToGenerate = 25;
            var ds = GenerateTrivial(itemsToGenerate);

            Assert.AreEqual(25, ds.LightList.Count());
            var first = ds.LightList.FirstOrDefault();
            Assert.AreEqual("Daniel Mettler", first.GetBestTitle());
        }

        public static DataTableDataSource GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            if(useCacheForSpeed && _cachedDs.ContainsKey(itemsToGenerate))
                return _cachedDs[itemsToGenerate];
            
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(DataTableDataSource.EntityIdDefaultColumnName, typeof (int)),
                new DataColumn("FullName"),
                new DataColumn("FirstName"),
                new DataColumn("LastName"),
                new DataColumn("City"),
                new DataColumn("IsMale", typeof (bool)),
                new DataColumn("Birthdate", typeof (DateTime)),
                new DataColumn("Height", typeof (int)),
                new DataColumn("CityMaybeNull", typeof(string)), 
                new DataColumn("InternalModified", typeof(DateTime)), 
            });
            AddSemirandomPersons(dataTable, itemsToGenerate, firstId);

            var source = new DataTableDataSource(dataTable, "Person", titleField: "FullName", modifiedField: "InternalModified")
            {
                ConfigurationProvider = new ValueProvider.ValueCollectionProvider_Test().ValueCollection()
            };

            // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            source.LightList.LastOrDefault();

            if (useCacheForSpeed)
                _cachedDs.Add(itemsToGenerate, source);
            return source;
        }

        private static void AddSemirandomPersons(DataTable dataTable, int itemsToGenerate = 10, int firstId = 1000)
        {
            for (var i = firstId; i < firstId + itemsToGenerate; i++)
            {
                var firstName = "First Name " + i;
                var lastName = "Last Name " + i;
                var fullName = firstName + " " + lastName;
                var city = TestCities[i%TestCities.Length];
                var cityMaybeNull = i % 2 == 0 ? null : city;
                var year = 1900 + i%110;
                var month = i%12+1;
                var day = i%28+1;
                var sysModified = RandomDate();// new DateTime(i % 7 + 1990 - i%11, i%11 + 1, (i + 20) % 28);
                dataTable.Rows.Add(i, 
                    fullName, 
                    firstName, 
                    lastName, 
                    city,
                    i % IsMaleForEveryX == 0, 
                    new DateTime(year, month, day),
                    MinHeight + i % HeightVar,
                    cityMaybeNull,
                    sysModified
                    );
            }
        }

        private static readonly Random Gen = new Random();
        private static DateTime RandomDate()
        {
            var start = new DateTime(1995, 1, 1);
            var range = (DateTime.Today - start).Days;
            return start.AddDays(Gen.Next(range));
        }


        public static DataTableDataSource GenerateTrivial(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(DataTableDataSource.EntityIdDefaultColumnName, typeof (int)),
                new DataColumn("EntityTitle"),
                new DataColumn("FirstName"),
                new DataColumn("LastName"),
                new DataColumn("City"),
                new DataColumn("InternalModified", typeof(DateTime)),
            });
            AddSemirandomTrivial(dataTable, itemsToGenerate, firstId);

            var source = new DataTableDataSource(dataTable, "Person", modifiedField: "InternalModified")
            {
                ConfigurationProvider = new ValueProvider.ValueCollectionProvider_Test().ValueCollection()
            };

            // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            source.LightList.LastOrDefault();

            if (useCacheForSpeed)
                _cachedDs.Add(itemsToGenerate, source);
            return source;
        }

        private static void AddSemirandomTrivial(DataTable dataTable, int itemsToGenerate = 10, int firstId = 1000)
        {
            for (var i = firstId; i < firstId + itemsToGenerate; i++)
            {
                var firstName = "Daniel";
                var lastName = "Mettler";
                var fullName = firstName + " " + lastName;
                var city = TestCities[i % TestCities.Length];
                var sysModified = RandomDate();
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
