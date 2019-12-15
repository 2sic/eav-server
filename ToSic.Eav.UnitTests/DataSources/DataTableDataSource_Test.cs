using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.TokenEngine.Tests.TestData;
using ToSic.Eav.TokenEngine.Tests.ValueProvider;
using DataTable = ToSic.Eav.DataSources.DataTable;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class DataTableDataSourceTest
    {
        public static string[] TestCities = { "Buchs", "Grabs", "Sevelen", "Zürich" };
        public static int MinHeight = 150;
        public static int HeightVar = 55;
        public static int IsMaleForEveryX = 3;

        private static readonly Dictionary<int, DataTable> _cachedDs = new Dictionary<int, DataTable>();

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
            Assert.IsTrue(defaultOut.List.Count() == itemsToGenerate);
        }

        [TestMethod]
        public void DataTable_CacheKey()
        {
            const int itemsToGenerate = 499;
            var ds = GeneratePersonSourceWithDemoData(itemsToGenerate);

            var expKey =
                "DataTableDataSource-NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person";
            Assert.AreEqual(expKey, ds.CachePartialKey);
            Assert.AreEqual(expKey, ds.CacheFullKey);
            var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
            Assert.IsTrue(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");
        }

        [TestMethod]
        public void DataTable_DefaultTitleField()
        {
            const int itemsToGenerate = 25;
            var ds = GenerateTrivial(itemsToGenerate);

            Assert.AreEqual(25, ds.List.Count());
            var first = ds.List.FirstOrDefault();
            Assert.AreEqual("Daniel Mettler", first.GetBestTitle());
        }

        public static DataTable GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            if(useCacheForSpeed && _cachedDs.ContainsKey(itemsToGenerate))
                return _cachedDs[itemsToGenerate];
            
            var dataTable = new System.Data.DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(DataTable.EntityIdDefaultColumnName, typeof (int)),
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

            var source = new DataTable(dataTable, "Person", titleField: "FullName", modifiedField: "InternalModified")
                .Init(DemoConfigs.AppSetAndRes());
            //{
            //    ConfigurationProvider = DemoConfigs.AppSetAndRes()
            //};

            // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            source.List.LastOrDefault();

            if (useCacheForSpeed)
                _cachedDs.Add(itemsToGenerate, source);
            return source;
        }

        private static void AddSemirandomPersons(System.Data.DataTable dataTable, int itemsToGenerate = 10, int firstId = 1000)
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


        public static DataTable GenerateTrivial(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            var dataTable = new System.Data.DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(DataTable.EntityIdDefaultColumnName, typeof (int)),
                new DataColumn("EntityTitle"),
                new DataColumn("FirstName"),
                new DataColumn("LastName"),
                new DataColumn("City"),
                new DataColumn("InternalModified", typeof(DateTime)),
            });
            AddSemirandomTrivial(dataTable, itemsToGenerate, firstId);

            var source = new DataTable(dataTable, "Person", modifiedField: "InternalModified")
                .Init(DemoConfigs.AppSetAndRes());
            //{
            //    ConfigurationProvider = DemoConfigs.AppSetAndRes()
            //};

            // now enumerate all, to be sure that the time counted for DS creation isn't part of the tracked test-time
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            source.List.LastOrDefault();

            if (useCacheForSpeed)
                _cachedDs.Add(itemsToGenerate, source);
            return source;
        }

        private static void AddSemirandomTrivial(System.Data.DataTable dataTable, int itemsToGenerate = 10, int firstId = 1000)
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
