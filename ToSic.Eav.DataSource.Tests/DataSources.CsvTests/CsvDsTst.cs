using System.ComponentModel;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.Helpers;
using ToSic.Eav.LookUp;
using ToSic.Eav.Testing;

namespace ToSic.Eav.DataSources.CsvTests;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
// ReSharper disable once InconsistentNaming
public class CsvDsTst_RerunIfFailed(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder)
{

    private const int TestFileRowCount = 40;

    private const int TestFileColumnCount = 5;

    private const string TestFileIdColumnName = "ID";

    private const string TestFileTitleColumnName = TitleNiceName;

    const string PathToCsvFiles = "Files/CsvDataSource";

    private string GetFullCsvPath(string path) =>
        TestFiles.GetTestPath("DataSources.CsvTests\\") + path.Backslash();

    [Fact]
    public void CsvDataSource_ParseSemicolonDelimitedFile()
    {
        var source = CreateCsvDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited.csv"),
            ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
        AssertIsSourceListValid(source);
    }

    [Fact]
    public void CsvDataSource_ParseSemicolonDelimitedUTF8File()
    {
        var source = CreateCsvDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited UTF8.csv"), 
            ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
        AssertIsSourceListValid(source);
    }

    [Fact]
    public void CsvDataSource_ParseTabDelimitedFile()
    {
        var source = CreateCsvDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Tab Delimited.csv"), 
            "\t", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
        AssertIsSourceListValid(source);
    }

    [Fact]
    [Description("Parses a file where texts are enquoted, for example 'Hello 2sic'.")]
    public void CsvDataSource_ParseFileWithQuotedText()
    {
        var source = CreateCsvDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Quoted Text.csv"),
            ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
        AssertIsSourceListValid(source);
    }

    [Fact]
    [Description("Parses a file and the name of the ID column is not defined - IDs should be taken from row numbers.")]
    public void CsvDataSource_ParseFileWithUndefinedIdColumnName()
    {
        var source = CreateCsvDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited.csv"),
            ";", "Anonymous", null, TestFileTitleColumnName);
        AssertIsSourceListValid(source);
    }

    [Fact]
    public void CsvDataSource_ParseFileWithIdColumnThatCannotBeParsed()
    {
        var source = CreateCsvDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Semicolon Delimited.csv"),
            ";", "Anonymous", TestFileTitleColumnName /* String cannot be parsed to Int */, TestFileTitleColumnName);
        // ReSharper disable once UnusedVariable
        var sourceList = source.ListTac();
        DataSourceErrors.VerifyStreamIsError(source, Csv.ErrorIdNaN);
    }

    [Fact]
    [Description("Parses a file where one row has not values for all columns - Test should succeed anyway.")]
    public void CsvDataSource_ParseFileWithInvalidRow()
    {
        var source = CreateCsvDataSource(GetFullCsvPath(PathToCsvFiles + " - Test Invalid Row.csv"),
            ";", "Anonymous", TestFileIdColumnName, TestFileTitleColumnName);
        AssertIsSourceListValid(source);
    }




    private void AssertIsSourceListValid(Csv source)
    {
        var sourceList = source.ListTac().OrderBy(item => item.EntityId).ToList();

        // List
        Equal(TestFileRowCount, sourceList.Count);//, "Entity list has not the expected length.");

        // Entities
        for (var i = 0; i < sourceList.Count; i++)
        {
            var entity = sourceList.ElementAt(i);

            Equal(TestFileColumnCount, entity.Attributes.Count);//, "Entity " + i + ": Attributes do not match the columns in the file.");
            if (string.IsNullOrEmpty(source.IdColumnName))
            {
                Equal(i + 2, entity.EntityId);//, "Entity " + i + ": ID does not match.");
            }
            else
            {
                Equal(GetAttributeValue(entity, source.IdColumnName), entity.EntityId.ToString());//, "Entity " + i + ": ID does not match.");
            }
            NotNull(GetAttributeValue(entity, source.TitleColumnName));//, "Entity " + i + ": Title should not be null.");
        }
    }

    private static object GetAttributeValue(IEntity entity, string name)
    {
        return entity.GetTac(entity.Attributes[name].Name);
    }

    public Csv CreateCsvDataSource(string filePath, string delimiter = ";", string contentType = "Anonymous", string idColumnName = null, string titleColumnName = null)
    {
        var source = dsSvc.CreateDataSource<Csv>(new LookUpTestData(dataBuilder).AppSetAndRes());

        source.FilePath = filePath;
        source.Delimiter = delimiter;
        source.ContentType = contentType;
        source.IdColumnName = idColumnName;
        source.TitleColumnName = titleColumnName;

        return source;
    }
}