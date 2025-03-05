using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests;


// ReSharper disable once InconsistentNaming
public class LogFn_Params : LogFn
{
    private const string FakeParameter = "param: test";
    /// <summary>
    /// Create a log for the test. Can be overriden in inheriting classes. 
    /// </summary>
    protected override (ILog LogForAdd, Log RealLog) LogFactory(string name = "")
    {
        var log = new Log(name);
        var fn = log.Fn(FakeParameter);
        return (fn, log);
    }

    protected override string WrapperSignature => $"{nameof(LogFactory)}({FakeParameter})";


}