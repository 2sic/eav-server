using ToSic.Lib.Logging;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests;

[TestClass]
public class LogCreate: LogTestBase
{
    // Tests to add
    // - Create with parameters
    // - Create with message
    // - Create with CodeRef
    // - etc.

    public static IEnumerable<object[]> LogNamesData => new[]
    {
        new object[] { "Name only", "test", "", "test", "test" },
        new object[] { "Short name only", "123", "", "123", "123" },
        new object[] { "Normal name and scope", "scp.name", "scp", "name", "scp.name" },
        new object[] { "Normal name and scope 2", "Scp.NameEx", "Scp", "NameEx", "Scp.NameEx" },
        new object[] { "Name too long", "muchtoolong", "", "muchto", "muchto" },
        new object[] { "Scope and Name too long", "tooLong.muchtoolong", "too", "muchto", "too.muchto" },
    };

    [TestMethod]
    [DynamicData(nameof(LogNamesData))]
    public void CreateLogWithName(string testName, string initName, string scope, string name,  string nameId)
    {
        var logWithInitialName = new Log(initName);
        VerifyNameAndEmpty($"{testName} initial name", logWithInitialName, scope, name, nameId);
    }

    [TestMethod]
    [DynamicData(nameof(LogNamesData))]
    public void CreateLogThenRename(string testName, string initName, string scope, string name,  string nameId)
    {
        var logRename = new Log("");
        logRename.Rename(initName);
        VerifyNameAndEmpty($"{testName} rename", logRename, scope, name, nameId);
    }

    [TestMethod]
    public void AddThenLink()
    {
        var child = new Log("Tst.Child");
        child.A("first message");
        child.A("second message");
        var parent = new Log("Tst.Parent");
        parent.A("first parent message");
        AreEqual(1, parent.Entries.Count);
        AreEqual(2, child.Entries.Count);
        child.LinkTo(parent);
        AreEqual(3, parent.Entries.Count);
        AreEqual(2, child.Entries.Count);
    }


    private static void VerifyNameAndEmpty(string testName, Log log, string scope, string name, string nameId)
    {
        AreEqual(scope, log.Scope, testName);
        AreEqual(name, log.Name, testName);
        AreEqual($"{nameId}[{log.Id}]", log.NameId, testName);
        AreEqual(Depth0, log.Depth, testName);
        AssertLogIsEmpty(log, testName);
    }
}