﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class DataTableDataSource_Test
    {
        public static string[] TestCities = new string[] { "Buchs", "Grabs", "Sevelen", "Zürich" };
        public static int MinHeight = 150;
        public static int HeightVar = 55;
        public static int IsMaleForEveryX = 3;

        [TestMethod]
        public void DataSource_Create_GeneralTest()
        {
            const int ItemsToGenerate = 499;
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(ItemsToGenerate);
            Assert.IsTrue(ds.In.Count == 0, "In count should be 0");
            Assert.IsTrue(ds.Out.Count == 1, "Out cound should be 1");
            var defaultOut = ds["Default"];
            Assert.IsTrue(defaultOut != null);
            try
            {
                var x = ds["Something"];
                Assert.Fail("Access to another out should fail");
            }
            catch { }
            Assert.IsTrue(defaultOut.List.Count == ItemsToGenerate);
        }

        [TestMethod]
        public void DataTable_CacheKey()
        {
            const int ItemsToGenerate = 499;
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(ItemsToGenerate);

            Assert.AreEqual("DataTableDataSource-NoGuid&ContentType=Person", ds.CachePartialKey);
            Assert.AreEqual("DataTableDataSource-NoGuid&ContentType=Person", ds.CacheFullKey);
            var lastRefresh = ds.CacheLastRefresh; // get this before comparison, because sometimes slow execution will get strange results
            Assert.IsTrue(DateTime.Now >= lastRefresh, "Date-check of cache refresh");
        }

        public static DataTableDataSource GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001)
        {
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
            });
            AddSemirandomPersons(dataTable, itemsToGenerate, firstId);

            var source = new DataTableDataSource(dataTable, "Person", titleField: "FullName");
            source.ConfigurationProvider = new ValueProvider.ValueCollectionProvider_Test().ValueCollection();

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
                dataTable.Rows.Add(i, 
                    fullName, 
                    firstName, 
                    lastName, 
                    city,
                    i % IsMaleForEveryX == 0, 
                    DateTime.Now.AddYears(-27), 
                    MinHeight + i % HeightVar,
                    cityMaybeNull
                    );
            }
        }


    }
}
