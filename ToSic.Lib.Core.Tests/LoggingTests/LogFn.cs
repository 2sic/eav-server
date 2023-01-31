using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    /// <summary>
    /// Note: you won't see any code, because it inherits all the tests from the base class.
    /// It just has a different way of getting the Log.
    /// </summary>
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class LogFn: LogTestBase
    {
        /// <summary>
        /// Create a log for the test. Can be overriden in inheriting classes. 
        /// </summary>
        protected virtual (ILog LogForAdd, Log RealLog) LogFactory(string name = "")
        {
            var log = new Log(name);
            var fn = log.Fn();
            return (fn, log);
        }

        protected virtual void Finish((ILog LogForAdd, Log RealLog) log) { /* do nothing in base class */ }

        protected virtual string WrapperSignature => $"{nameof(LogFactory)}()";

        // protected override int LogDepth => 1;
        /// <summary>
        /// How deep the log is - if it has parents etc.
        /// </summary>
        protected virtual int LogDepth => 0;

        /// <summary>
        /// Expected amount of entries
        /// </summary>
        protected virtual int EntryCount => 2;

        protected virtual bool DoResultCheck => false;
        protected virtual string? ExpectedResult => null;

        public static IEnumerable<object?[]> SimpleMessages => TestDataMessages.SimpleMessages(Depth0);

        private void CheckResult(string testName, Log log, int indexWrap, int indexResult, int depth)
        {
            if (!DoResultCheck) return;
            var messageToNotTestIt = log.Entries[indexWrap].Message;
            AssertEntry(testName, log.Entries[indexWrap], messageToNotTestIt, ExpectedResult, depth);
            AssertEntry(testName, log.Entries[indexResult], null, ExpectedResult, depth);
        }

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void NoAdd(string testName, string expected, string message, string result, int depth)
        {
            AssertDepthAndEntryCount(testName, LogFactory().RealLog, LogDepth, 1);
            AssertEntry(testName, LogFactory().RealLog.Entries[0], WrapperSignature, result, depth);
        }

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void A_NotEnabled(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.LogForAdd.A(false, message);
            Finish(log);
            AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount - 1);
            AssertEntry(testName, log.RealLog.Entries[0], WrapperSignature, ExpectedResult, depth);
            CheckResult(testName, log.RealLog, 0, 1, depth);
        }


        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void A_String(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.LogForAdd.A(message);
            Finish(log);
            AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount);
            AssertEntry(testName, log.RealLog.Entries[0], WrapperSignature, ExpectedResult, depth);
            AssertEntry(testName, log.RealLog.Entries[1], expected, result, depth + 1);
            CheckResult(testName, log.RealLog, 0, 2, depth);
        }


        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void A_StringFunction(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.LogForAdd.A(log.LogForAdd.Try(() => message));
            Finish(log);
            AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount);
            AssertEntry(testName, log.RealLog.Entries[0], WrapperSignature, ExpectedResult, depth);
            AssertEntry(testName, log.RealLog.Entries[1], expected, result, depth + 1);
            CheckResult(testName, log.RealLog, 0, 2, depth);
        }

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void W_String(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.LogForAdd.W(message);
            Finish(log);
            AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount);
            AssertEntry(testName, log.RealLog.Entries[0], WrapperSignature, ExpectedResult, depth);
            AssertEntry(testName, log.RealLog.Entries[1], LogConstants.WarningPrefix + expected, result, depth + 1);
            CheckResult(testName, log.RealLog, 0, 2, depth);
        }

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void E_String(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.LogForAdd.E(message);
            Finish(log);
            AssertDepthAndEntryCount(testName, log.RealLog, LogDepth, EntryCount);
            AssertEntry(testName, log.RealLog.Entries[0], WrapperSignature, ExpectedResult, depth);
            AssertEntry(testName, log.RealLog.Entries[1], LogConstants.ErrorPrefix + expected, result, depth + 1);
            CheckResult(testName, log.RealLog, 0, 2, depth);
        }
    }
}