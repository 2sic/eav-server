using ToSic.Lib.Logging;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    [TestClass]
    public class LogAdd: LogTestBase
    {
        /// <summary>
        /// Create a log for the test. Can be overriden in inheriting classes. 
        /// </summary>
        protected virtual Log LogFactory(string name = "") => new(name);

        /// <summary>
        /// How deep the log is - if it has parents etc.
        /// </summary>
        protected virtual int LogDepth => 0;

        /// <summary>
        /// Expected amount of entries
        /// </summary>
        protected virtual int EntryCount => 1;

        public static IEnumerable<object?[]> SimpleMessages => TestDataMessages.SimpleMessages(Depth0);

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void A_String(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.A(message);
            AssertDepthAndEntryCount(testName, log, LogDepth, EntryCount);
            AssertEntry(testName, log.Entries[0], expected, result, depth);
        }


        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void A_NotEnabled(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.A(false, message);
            AssertDepthAndEntryCount(testName, log, LogDepth, 0);
        }

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void A_StringFunction(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.A(() => message);
            AssertDepthAndEntryCount(testName, log, LogDepth, EntryCount);
            AssertEntry(testName, log.Entries[0], expected, result, depth);
        }

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void W_String(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.W(message);
            AssertDepthAndEntryCount(testName, log, LogDepth, EntryCount);
            AssertEntry(testName, log.Entries[0], LogConstants.WarningPrefix + expected, result, depth);
        }

        [TestMethod]
        [DynamicData(nameof(SimpleMessages))]
        public void E_String(string testName, string expected, string message, string result, int depth)
        {
            var log = LogFactory();
            log.E(message);
            AssertDepthAndEntryCount(testName, log, LogDepth, EntryCount);
            AssertEntry(testName, log.Entries[0], LogConstants.ErrorPrefix + expected, result, depth);
        }

    }
}