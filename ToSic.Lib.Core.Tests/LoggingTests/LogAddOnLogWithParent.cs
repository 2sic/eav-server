using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests;

/// <summary>
/// Note: you won't see any code, because it inherits all the tests from the base class.
/// It just has a different way of getting the Log.
/// </summary>
[TestClass]
public class LogAddOnLogWithParent : LogAdd
{
    /// <summary>
    /// Create a log for the test. Can be overriden in inheriting classes. 
    /// </summary>
    protected override (ILog LogForAdd, Log RealLog) LogFactory(string name = "")
    {
        var log = new Log("", new Log(""));
        return (log, log);
    }

    protected override int LogDepth => 1;
}