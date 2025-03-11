

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Security.Files.Tests;

[TestClass()]
public class SanitizeFileNameTests
{
    private static string SanitizeFileName(string fileName) => FileNames.SanitizeFileName(fileName);
    private const string SafeChar = FileNames.SafeChar;



    [TestMethod()]
    [DataRow("valid-file-name.exe", "valid-file-name.exe")]
    [DataRow("valid.file...name.exe", "valid.file...name.exe")]
    [DataRow(".valid-file-name.", ".valid-file-name.")]
    [DataRow(null, SafeChar)]
    [DataRow("", SafeChar)]
    [DataRow("    ", SafeChar)]
    [DataRow("nUl", SafeChar)]
    [DataRow("com3", SafeChar)]
    [DataRow("auX", SafeChar)]
    [DataRow("//server/path/should/not/be/here/file", "file")]
    [DataRow(@"C:\\path\should\not\be\here\file", "file")]
    [DataRow(@"..\..\path\not\like\file", "file")]
    [DataRow(@".\file", "file")]
    [DataRow("something:file", "file")]
    [DataRow(@"/file/", SafeChar)]
    [DataRow("file:", SafeChar)]
    [DataRow("bla/bla/?com3*", SafeChar)]
    [DataRow(@"+%#file#%++", "plusperhashfilehashperplusplus")]
    [DataRow("file*.*", "file_.")]
    [DataRow("file?.???", "file_.")]
    public void SanitizeFileNameTest(string actual, string expected)
    {
        Assert.AreEqual(expected, SanitizeFileName(actual));
    }



    [TestMethod()]
    [DataRow("file\"")]
    [DataRow("file<")]
    [DataRow("file>")]
    [DataRow("file|")]
    [DataRow("file\n")]
    [DataRow("file\t")]
    [DataRow("file\a")]
    [DataRow("file\b")]
    [DataRow("file\r")]
    [DataRow("file\u0001")]
    [DataRow("file\u000E")]
    [DataRow("file\u001F")]
    public void SanitizeFileNameExceptionTest(string actual)
    {
        Assert.ThrowsException<System.ArgumentException> (() => SanitizeFileName(actual));
    }
}