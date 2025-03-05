using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests;

[TestClass]
public class LogTests
{
    [TestMethod]
    public void LogTest()
    {
        var log = new Lib.Logging.Log("test");
        log.A("test-add");
        Assert.AreEqual(1, log.Entries.Count, "should have one entry");
    }
}