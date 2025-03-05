using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests;


// ReSharper disable once InconsistentNaming
public class LogFn_BoolReturnTrueAndLog : LogFn_BoolReturnTrue
{
    protected override void Finish((ILog LogForAdd, Log RealLog) log) => (log.LogForAdd as ILogCall<bool>)?.ReturnAndLog(true);

    protected override string? ExpectedResult => true.ToString();
}