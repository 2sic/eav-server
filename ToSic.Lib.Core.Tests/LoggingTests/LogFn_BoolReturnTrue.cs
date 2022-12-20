using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class LogFn_BoolReturnTrue : LogFn
    {
        /// <summary>
        /// Create a log for the test. Can be overriden in inheriting classes. 
        /// </summary>
        protected override (ILog LogForAdd, Log RealLog) LogFactory(string name = "")
        {
            var log = new Log(name);
            var fn = log.Fn<bool>();
            return (fn, log);
        }

        protected override void Finish((ILog LogForAdd, Log RealLog) log) => (log.LogForAdd as LogCall<bool>)?.Return(true);

        protected override int EntryCount => 3;

        protected override bool DoResultCheck => true;
        protected override string? ExpectedResult => null;
    }
}