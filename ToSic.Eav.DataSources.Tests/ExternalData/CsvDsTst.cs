using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Helpers;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.ExternalData
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class CsvDsTst_RerunIfFailed: TestBaseDiEavFull
    {
        private const int TestFileRowCount = 40;

        private const int TestFileColumnCount = 5;

        private const string TestFileIdColumnName = "ID";

        private const string TestFileTitleColumnName = "Title";

        const string PathToCsvFiles = "Files/CsvDataSource";

        private string GetFullCsvPath(string path)
        {
            // todo: figure out path to current system #todotest
            return "C:\\Projects\\2sxc\\eav-server\\ToSic.Eav.DataSources.Tests\\" + path.Backslash();
        }

        [TestMethod]
        public void CsvDataSource_ParseSemicolonDelimitedFile()
        {
            var source = CreateDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited.csv"),
                ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        public void CsvDataSource_ParseSemicolonDelimitedUTF8File()
        {
            var source = CreateDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited UTF8.csv"), 
                ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        public void CsvDataSource_ParseTabDelimitedFile()
        {
            var source = CreateDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Tab Delimited.csv"), 
                "\t", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [Description("Parses a file where texts are enquoted, for example 'Hello 2sic'.")]
        public void CsvDataSource_ParseFileWithQuotedText()
        {
            var source = CreateDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Quoted Text.csv"),
                ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        [Description("Parses a file and the name of the ID column is not defined - IDs should be taken from row numbers.")]
        public void CsvDataSource_ParseFileWithUndefinedIdColumnName()
        {
            var source = CreateDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited.csv"),
                ";", "Anonymous", null, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }

        [TestMethod]
        public void CsvDataSource_ParseFileWithIdColumnThatCannotBeParsed()
        {
            var source = CreateDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited.csv"),
                ";", "Anonymous", TestFileTitleColumnName /* String cannot be parsed to Int */, TestFileTitleColumnName);
            // ReSharper disable once UnusedVariable
            var sourceList = source.ListForTests();
            DataSourceErrors.VerifyStreamIsError(source, CsvDataSource.ErrorIdNaN);
        }

        [TestMethod]
        [Description("Parses a file where one row has not values for all columns - Test should succeed anyway.")]
        public void CsvDataSource_ParseFileWithInvalidRow()
        {
            var source = CreateDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Invalid Row.csv"),
                ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
            AssertIsSourceListValid(source);
        }




        private void AssertIsSourceListValid(CsvDataSource source)
        {
            var sourceList = source.ListForTests().OrderBy(item => item.EntityId).ToList();

            // List
            Assert.AreEqual(TestFileRowCount, sourceList.Count, "Entity list has not the expected length.");

            // Entities
            for (var i = 0; i < sourceList.Count; i++)
            {
                var entity = sourceList.ElementAt(i);

                Assert.AreEqual(TestFileColumnCount, entity.Attributes.Count, "Entity " + i + ": Attributes do not match the columns in the file.");
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

        private static object GetAttributeValue(IEntity entity, string name)
        {
            return entity.Value(entity.Attributes[name].Name);
        }

        public CsvDataSource CreateDataSource(string filePath, string delimiter = ";", string contentType = "Anonymous", string idColumnName = null, string titleColumnName = null)
        {
            var source = this.GetTestDataSource<CsvDataSource>(); // EavTestBase.Resolve<CsvDataSource>();

            source.FilePath = filePath;
            source.Delimiter = delimiter;
            source.ContentType = contentType;
            source.IdColumnName = idColumnName;
            source.TitleColumnName = titleColumnName;
            //source.ConfigurationProvider = DemoConfigs.AppSetAndRes()


            return source.Init(LookUpTestData.AppSetAndRes());
        }
    }
}
