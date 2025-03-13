using ToSic.Eav.Security.Files;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Security.Files;


public class SanitizeFileNameTests(ITestOutputHelper output)
{
    private static string SanitizeFileNameTac(string fileName) => FileNames.SanitizeFileName(fileName);
    private const string SafeChar = FileNames.SafeChar;

    [Theory]
    [InlineData("valid-file-name.exe", "valid-file-name.exe")]
    [InlineData("valid.file...name.exe", "valid.file...name.exe")]
    [InlineData(".valid-file-name.", ".valid-file-name.")]
    [InlineData(null, SafeChar)]
    [InlineData("", SafeChar)]
    [InlineData("    ", SafeChar)]
    [InlineData("nUl", SafeChar)]
    [InlineData("com3", SafeChar)]
    [InlineData("auX", SafeChar)]
    [InlineData("//server/path/should/not/be/here/file", "file")]
    [InlineData(@"C:\\path\should\not\be\here\file", "file")]
    [InlineData(@"..\..\path\not\like\file", "file")]
    [InlineData(@".\file", "file")]
    [InlineData("something:file", "file", "something_file")]
    [InlineData(@"/file/", SafeChar)]
    [InlineData("file:", SafeChar, "file")]
    [InlineData("bla/bla/?com3*", SafeChar)]
    [InlineData(@"+%#file#%++", "plusperhashfilehashperplusplus")]
    [InlineData("file*.*", "file_.")]
    [InlineData("file?.???", "file_.")]
    public void SanitizeFileNameTest(string actual, string expected, string? expectedNetCore = null)
    {
#if NETCOREAPP
        Equal(expectedNetCore ?? expected, SanitizeFileNameTac(actual));
#else
        Equal(expected, SanitizeFileNameTac(actual));
#endif
    }


    [Theory]
    [InlineData("file\"")]
    [InlineData("file<")]
    [InlineData("file>")]
    [InlineData("file|")]
    [InlineData("file\n")]
    [InlineData("file\t")]
    [InlineData("file\a")]
    [InlineData("file\b")]
    [InlineData("file\r")]
    [InlineData("file\u0001")]
    [InlineData("file\u000E")]
    [InlineData("file\u001F")]
    public void SanitizeFileNameExceptionTest(string actual)
    {
        // .net Framework will throw an exception, because `Path.GetFileName(...)` will complain.
        // .net core 9 will simply drop that part of the name
#if NETCOREAPP
        Equal("file", SanitizeFileNameTac(actual));
#else
        Throws<ArgumentException>(() =>
        {
            // This should already throw an error
            var result = SanitizeFileNameTac(actual);

            // if not, trace for better debugging
            output.WriteLine($"Result: '{result}'");
        });
#endif
    }
}