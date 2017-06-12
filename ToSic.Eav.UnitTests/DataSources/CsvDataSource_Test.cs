using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.UnitTests.ValueProvider;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class CsvDataSource_Test
    {
        private const int TestFileRowCount = 40;

        private const int TestFileColumnCount = 5;

        private const string TestFileIdColumnName = "ID";

        private const string TestFileTitleColumnName = "Title";


        [TestMethod]
        public void CsvDataSource_ParseSemicolonDelimitedFile()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        public void CsvDataSource_ParseSemicolonDelimitedUTF8File()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited UTF8.csv", ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        public void CsvDataSource_ParseTabDelimitedFile()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Tab Delimited.csv", "\t", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [Description("Parses a file where texts are enquoted, for example 'Hello 2sic'.")]
        public void CsvDataSource_ParseFileWithQuotedText()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Quoted Text.csv", ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [Description("Parses a file and the name of the ID column is not defined - IDs should be taken from row numbers.")]
        public void CsvDataSource_ParseFileWithUndefinedIdColumnName()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", null, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CsvDataSource_ParseFileWithIdColumnThatCannotBeParsed()
        {
            try
            {
                var source = CreateDataSource("Files/CsvDataSource - Test Semicolon Delimited.csv", ";", "Anonymous", TestFileTitleColumnName /* String cannot be parsed to Int */, TestFileTitleColumnName);
                // ReSharper disable once UnusedVariable
                var sourceList = source.LightList;
            }
            catch (Exception ex)
            {       
                // The pipeline does wrap my exception expected
                throw ex.InnerException;
            }
        }

        [TestMethod]
        [Description("Parses a file where one row has not values for all columns - Test should succeed anyway.")]
        public void CsvDataSource_ParseFileWithInvalidRow()
        {
            var source = CreateDataSource("Files/CsvDataSource - Test Invalid Row.csv", ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }




        private void AssertIsSourceListValid(CsvDataSource source)
        {
            var sourceList = source.LightList.OrderBy(item => item.EntityId).ToList();

            // List
            Assert.AreEqual(sourceList.Count(), TestFileRowCount, "Entity list has not the expected length.");

            // Entities
            for (var i = 0; i < sourceList.Count(); i++)
            {
                var entity = sourceList.ElementAt(i);

                Assert.AreEqual(TestFileColumnCount, entity.Attributes.Count(), "Entity " + i + ": Attributes do not match the columns in the file.");
                if (string.IsNullOrEmpty(source.IdColumnName))
                {
                    Assert.AreEqual(i + 2, entity.EntityId, "Entity " + i + ": ID does not match.");
                }
                else
                {
                    Assert.AreEqual(GetAttributeValue(entity, source.IdColumnName), entity.EntityId.ToString(), "Entity " + i + ": ID does not match.");
                }
                Assert.IsNotNull(GetAttributeValue(entity, source.TitleColumnName), "Entity " + i + ": Title should not be null.");
            }
        }

        private static object GetAttributeValue(ToSic.Eav.Interfaces.IEntity entity, string name)
        {
            return entity.GetBestValue(entity.Attributes[name].Name);
        }

        public static CsvDataSource CreateDataSource(string filePath, string delimiter = ";", string contentType = "Anonymous", string idColumnName = null, string titleColumnName = null)
        {
            var source = new CsvDataSource
            {
                FilePath = filePath,
                Delimiter = delimiter,
                ContentType = contentType,
                IdColumnName = idColumnName,
                TitleColumnName = titleColumnName,
                ConfigurationProvider = new ValueCollectionProvider_Test().ValueCollection()
            };
            return source;
        }
    }
}
