using ToSic.Lib.Logging;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    public class LogTestBase
    {
        public const int Depth0 = 0;
        public const string? ResultNone = null;

        protected static void AssertLogIsEmpty(Log log, string testName)
        {
            AreEqual(0, log.Entries.Count, testName);
        }

        protected static void AssertEntry(string testName, Entry firstMsg, string message, string result, int depth)
        {
            AreEqual(message, firstMsg.Message, testName);
            AreEqual(result, firstMsg.Result, testName);
            AreEqual(depth, firstMsg.Depth, testName);
        }

        internal void AssertDepthAndEntryCount(string testName, Log log, int logDepth, int entryCount)
        {
            AreEqual(logDepth, log.Depth, testName);
            AreEqual(entryCount, log.Entries.Count, testName);
        }

    }
}
