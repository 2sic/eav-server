using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests;

[TestClass]
// ReSharper disable once InconsistentNaming
public class LogFn_DoneOk : LogFn
{
    protected override void Finish((ILog LogForAdd, Log RealLog) log)
    {
        (log.LogForAdd as ILogCall).Done(ExpectedResult);
    }

    protected override int EntryCount => 3;

    protected override bool DoResultCheck => true;
    protected override string? ExpectedResult => "ok";
}