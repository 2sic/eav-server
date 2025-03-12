using ToSic.Eav.Security.Files;

namespace ToSic.Eav.ImportExport.Tests19.Security.Files;


public class SanitizeFileNameTests
{
    private static string SanitizeFileName(string fileName) => FileNames.SanitizeFileName(fileName);
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
    [InlineData("something:file", "file")]
    [InlineData(@"/file/", SafeChar)]
    [InlineData("file:", SafeChar)]
    [InlineData("bla/bla/?com3*", SafeChar)]
    [InlineData(@"+%#file#%++", "plusperhashfilehashperplusplus")]
    [InlineData("file*.*", "file_.")]
    [InlineData("file?.???", "file_.")]
    public void SanitizeFileNameTest(string actual, string expected) =>
        Equal(expected, SanitizeFileName(actual));


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
    public void SanitizeFileNameExceptionTest(string actual) =>
        Throws<ArgumentException> (() => SanitizeFileName(actual));
}