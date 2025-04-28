namespace ToSic.Lib.Core.Tests.LoggingTests;


public class LogCreate: LogTestBase
{
    // Tests to add
    // - Create with parameters
    // - Create with message
    // - Create with CodeRef
    // - etc.

    public static IEnumerable<object[]> LogNamesData =>
    [
        ["Name only", "test", "", "test", "test"],
        ["Short name only", "123", "", "123", "123"],
        ["Normal name and scope", "scp.name", "scp", "name", "scp.name"],
        ["Normal name and scope 2", "Scp.NameEx", "Scp", "NameEx", "Scp.NameEx"],
        ["Name too long", "muchtoolong", "", "muchto", "muchto"],
        ["Scope and Name too long", "tooLong.muchtoolong", "too", "muchto", "too.muchto"]
    ];

    [Theory]
    [MemberData(nameof(LogNamesData))]
    public void CreateLogWithName(string testName, string initName, string scope, string name,  string nameId)
    {
        var logWithInitialName = new Log(initName);
        VerifyNameAndEmpty($"{testName} initial name", logWithInitialName, scope, name, nameId);
    }

    [Theory]
    [MemberData(nameof(LogNamesData))]
    public void CreateLogThenRename(string testName, string initName, string scope, string name,  string nameId)
    {
        var logRename = new Log("");
        logRename.Rename(initName);
        VerifyNameAndEmpty($"{testName} rename", logRename, scope, name, nameId);
    }

    [Fact]
    public void AddThenLink()
    {
        var child = new Log("Tst.Child");
        child.A("first message");
        child.A("second message");
        var parent = new Log("Tst.Parent");
        parent.A("first parent message");
        Single(parent.Entries);
        Equal(2, child.Entries.Count);
        child.LinkTo(parent);
        Equal(3, parent.Entries.Count);
        Equal(2, child.Entries.Count);
    }


    private static void VerifyNameAndEmpty(string testName, Log log, string scope, string name, string nameId)
    {
        Equal(scope, log.Scope);
        Equal(name, log.Name);
        Equal($"{nameId}[{log.Id}]", log.NameId);
        Equal(Depth0, log.Depth);
        AssertLogIsEmpty(log, name);
    }
}