using ToSic.Lib.Logging;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class Log_Do: LogTestBase
    {
        [TestMethod]
        public void Do_Basic()
        {
            var log = new Log("tst.Test");
            var x = 0;
            log.Do(() => { x = 7; });
            AreEqual(7, x);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
        }

        [TestMethod]
        public void Do_EnsureNullSafe()
        {
            var log = null as ILog;
            var x = 0;
            log.Do(() => { x = 7; });
            AreEqual(7, x);
        }

        [TestMethod]
        public void DoParameters_Basic()
        {
            var log = new Log("tst.Test");
            var x = 0;
            log.Do("id: 7", () => { x = 7; });
            AreEqual(7, x);
            AreEqual($"{nameof(DoParameters_Basic)}(id: 7)", log.Entries[0].Message);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
        }


        [TestMethod]
        public void Do_WithInnerAddOnParent()
        {
            var log = new Log("tst.Test");
            var x = 0;
            log.Do(() =>
            {
                x = 7;
                log.A("nice");
            });
            AreEqual(7, x);
            AreEqual(3, log.Entries.Count);
        }
        [TestMethod]
        public void Do_WithInnerOnChild()
        {
            var parentLog = new Log("tst.Test");
            var x = 0;
            parentLog.Do(log =>
            {
                x = 7;
                log.A("nice");
            });
            AreEqual(7, x);
            AreEqual(3, parentLog.Entries.Count);
        }

        [TestMethod]
        public void Do_InDo_Parent()
        {
            var parentLog = new Log("tst.Test");
            var x = 0;
            parentLog.Do(() =>
            {
                x = 7;
                parentLog.Do(() => { x = 9; });
            });
            AreEqual(9, x);
            AreEqual(2 * 2, parentLog.Entries.Count);
        }

        [TestMethod]
        public void Do_InDo_Inner()
        {
            var parentLog = new Log("tst.Test");
            var x = 0;
            parentLog.Do(l =>
            {
                x = 7;
                l.Do(() => { x = 9; });
            });
            AreEqual(9, x);
            AreEqual(2 * 2, parentLog.Entries.Count);
        }

        [TestMethod]
        public void DoAndReturnMessage_Basic()
        {
            var resultMessage = "this is ok";
            var log = new Log("tst.Test");
            var x = 0;
            log.Do(() =>
            {
                x = 7;
                return resultMessage;
            });
            AreEqual(7, x);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual(resultMessage, log.Entries[0].Result);
        }

        [TestMethod]
        public void DoAndReturnMessage_WithInnerAdd()
        {
            var resultMessage = "this is ok";
            var log = new Log("tst.Test");
            var x = 0;
            log.Do(() =>
            {
                x = 7;
                log.A("nice");
                return resultMessage;
            });
            AreEqual(7, x);
            AreEqual(3, log.Entries.Count);
            AreEqual(resultMessage, log.Entries[0].Result);
        }

    }
}
