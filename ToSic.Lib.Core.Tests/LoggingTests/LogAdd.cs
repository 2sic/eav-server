namespace ToSic.Lib.Core.Tests.LoggingTests;


public class LogAdd: LogTestBase
{
    /// <summary>
    /// Create a log for the test. Can be overriden in inheriting classes. 
    /// </summary>
    protected virtual (ILog LogForAdd, Log RealLog) LogFactory(string name = "")
    {
        var log = new Log(name);
        return (log, log);
    }

    /// <summary>
    /// How deep the log is - if it has parents etc.
    /// </summary>
    protected virtual int LogDepth => 0;

    /// <summary>
    /// Expected amount of entries
    /// </summary>
    protected virtual int EntryCount => 1;

    public static IEnumerable<object?[]> SimpleMessages => TestDataMessages.SimpleMessages(Depth0);

    [Theory]
    [MemberData(nameof(SimpleMessages))]
    public void A_String(string testName, string expected, string message, string result, int depth)
    {
        var log = LogFactory();
        log.LogForAdd.A(message);
        AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount);
        AssertEntry(testName, log.RealLog.Entries[0], expected, result, depth);
    }


    [Theory]
    [MemberData(nameof(SimpleMessages))]
    public void A_NotEnabled(string testName, string expected, string message, string result, int depth)
    {
        var log = LogFactory();
        log.LogForAdd.A(false, message);
        AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, 0);
    }

    [Theory]
    [MemberData(nameof(SimpleMessages))]
    public void A_StringFunction(string testName, string expected, string message, string result, int depth)
    {
        var logFactory = LogFactory();
        logFactory.LogForAdd.A(logFactory.LogForAdd.Try(() => message));
        AssertDepthAndEntryCount(testName, logFactory.RealLog, LogDepth, EntryCount);
        AssertEntry(testName, logFactory.RealLog.Entries[0], expected, result, depth);
    }

    [Theory]
    [MemberData(nameof(SimpleMessages))]
    public void W_String(string testName, string expected, string message, string result, int depth)
    {
        var log = LogFactory();
        log.LogForAdd.W(message);
        AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount);
        AssertEntry(testName, log.RealLog.Entries[0], LogConstants.WarningPrefix + expected, result, depth);
    }

    [Theory]
    [MemberData(nameof(SimpleMessages))]
    public void E_String(string testName, string expected, string message, string result, int depth)
    {
        var log = LogFactory();
        log.LogForAdd.E(message);
        AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount);
        AssertEntry(testName, log.RealLog.Entries[0], LogConstants.ErrorPrefix + expected, result, depth);
    }

}