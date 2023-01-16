using ToSic.Lib.Logging;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class Log_Func: LogTestBase
    {
        [TestMethod]
        public void EnsureNullSafe()
        {
            var log = null as ILog;
            var x = log.Func(() => 7);
            AreEqual(7, x);
        }

        [TestMethod]
        public void Func_ReturnNull()
        {
            var log = new Log("test");
            var x = log.Func(() => null as IEnumerable<string>);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual("", log.Entries[0].Result);
            IsNull(x);
        }

        [TestMethod]
        public void Func_ReturnValue()
        {
            var log = new Log("tst.Test");
            var x = log.Func(() => 7);
            AreEqual(7, x);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            var header = log.Entries[0];
            AreEqual("7", header.Result);
            AreEqual($"{nameof(Func_ReturnValue)}()", header.Message);
        }

        [TestMethod]
        public void Func_ReturnDateTime()
        {
            var expected = new DateTime();
            var log = new Log("test");
            var x = log.Func(() => expected);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual(expected.ToString(), log.Entries[0].Result);
            AreEqual(expected, x);
        }

        // TODO: TEST RETURNVALUE-TIMER

        [TestMethod]
        public void Func_Params_ReturnValue()
        {
            var log = new Log("test");
            var x = log.Func("id: 7", () => 7);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{nameof(Func_Params_ReturnValue)}(id: 7)", log.Entries[0].Message);
            AreEqual("7", log.Entries[0].Result);
        }

        [TestMethod]
        public void Func_ReturnValueAndMessage()
        {
            var log = new Log("tst.Test");
            var x = log.Func(() => (7, "ok"));
            AreEqual(7, x);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            var header = log.Entries[0];
            AreEqual("7 - ok", header.Result);
            AreEqual($"{nameof(Func_ReturnValueAndMessage)}()", header.Message);
        }

        [TestMethod]
        public void Func_Params_ReturnNumber()
        {
            var log = new Log("test");
            var x = log.Func("7", func: () => 7);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{nameof(Func_ParamsMessage_ReturnNumber)}(7)", log.Entries[0].Message);
            AreEqual("7", log.Entries[0].Result);
        }


        [TestMethod]
        public void Func_ParamsMessage_ReturnNumber()
        {
            var log = new Log("test");
            var x = log.Func("7", message: "get 7", func: () => 7);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{nameof(Func_ParamsMessage_ReturnNumber)}(7) get 7", log.Entries[0].Message);
            AreEqual("7", log.Entries[0].Result);
        }

        [TestMethod]
        public void Func_ParamsMessage_ReturnNumberAndMessage()
        {
            var log = new Log("test");
            var x = log.Func("7", message: "get 7", func: () => (7, "all ok"));
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{nameof(Func_ParamsMessage_ReturnNumberAndMessage)}(7) get 7", log.Entries[0].Message);
            AreEqual("7 - all ok", log.Entries[0].Result);
        }


    }
}
