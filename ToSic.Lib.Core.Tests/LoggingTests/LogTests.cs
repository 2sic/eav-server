using ToSic.Lib.Logging;
using static Xunit.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests;


public class LogTests
{
    [Fact]
    public void LogTest()
    {
        var log = new Log("test");
        log.A("test-add");
        Single(log.Entries); //, "should have one entry");
    }
}