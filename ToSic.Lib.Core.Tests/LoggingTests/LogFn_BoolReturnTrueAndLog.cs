using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class LogFn_BoolReturnTrueAndLog : LogFn_BoolReturnTrue
    {
        protected override void Finish((ILog LogForAdd, Log RealLog) log) => (log.LogForAdd as LogCall<bool>)?.ReturnAndLog(true);

        protected override string? ExpectedResult => true.ToString();
    }
}