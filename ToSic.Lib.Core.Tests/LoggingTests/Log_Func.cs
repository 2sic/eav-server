﻿using ToSic.Lib.Logging;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class Log_Func: LogTestBase
    {
        #region Basic Tests

        [TestMethod]
        public void Basic_EnsureNullSafe()
        {
            var log = null as ILog;
            var x = log.Func(() => 7);
            AreEqual(7, x);
        }

        #endregion

        #region Return Values


        [TestMethod] public void Func_ReturnNull() => 
#pragma warning disable CS8625
            Func_ReturnValue_Assert<string>(ThisMethodName(), log => log.Func(() => null as string)!, null, 2);
#pragma warning restore CS8625

        [TestMethod] public void Func_ReturnValue() => 
            Func_ReturnValue_Assert(ThisMethodName(), log => log.Func(() => 7), 7, 2);

        [TestMethod] public void Func_EnabledTrue_ReturnValue() => 
            Func_ReturnValue_Assert(ThisMethodName(), log => log.Func(() => 27, enabled: true), 27, 2);

        [TestMethod] public void Func_EnabledFalse_ReturnValue() => 
            Func_ReturnValue_Assert(ThisMethodName(), log => log.Func(() => 42, enabled: false), 42, 0);

        private static void Func_ReturnValue_Assert<T>(string fnName, Func<Log, T> logFunc, T expected, int expectedCount)
        {
            var log = new Log("tst.Test");
            var x = logFunc(log);
            AreEqual(expected, x);
            AreEqual(expectedCount, log.Entries.Count, "should have two entries (start/stop)");
            if (log.Entries.Any())
            {
                var header = log.Entries[0];
                AreEqual(expected?.ToString() ?? "", header.Result);
                AreEqual($"{fnName}()", header.Message);
            }
        }

        [TestMethod] public void Func_ReturnDateTime()
        {
            var expected = new DateTime();
            var log = new Log("test");
            var x = log.Func(() => expected);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual(expected.ToString(), log.Entries[0].Result);
            AreEqual(expected, x);
        }

        #endregion

        [TestMethod] public void Func_ReturnValueAndMessage()
        {
            var log = new Log("tst.Test");
            var x = log.Func(() => (7, "ok"));
            AreEqual(7, x);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            var header = log.Entries[0];
            AreEqual("7 - ok", header.Result);
            AreEqual($"{ThisMethodName()}()", header.Message);
        }

        #region TODO: timer

        #endregion


        #region With Params

        [TestMethod]
        public void Func_Params_ReturnValue()
        {
            var log = new Log("test");
            var x = log.Func("id: 7", () => 7);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{ThisMethodName()}(id: 7)", log.Entries[0].Message);
            AreEqual("7", log.Entries[0].Result);
        }


        [TestMethod]
        public void Func_Params_ReturnNumber()
        {
            var log = new Log("test");
            var x = log.Func("7", func: () => 7);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{ThisMethodName()}(7)", log.Entries[0].Message);
            AreEqual("7", log.Entries[0].Result);
        }


        [TestMethod]
        public void Func_ParamsMessage_ReturnNumber()
        {
            var log = new Log("test");
            var x = log.Func("7", message: "get 7", func: () => 7);
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{ThisMethodName()}(7) get 7", log.Entries[0].Message);
            AreEqual("7", log.Entries[0].Result);
        }

        [TestMethod]
        public void Func_ParamsMessage_ReturnNumberAndMessage()
        {
            var log = new Log("test");
            var x = log.Func("7", message: "get 7", func: () => (7, "all ok"));
            AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
            AreEqual($"{ThisMethodName()}(7) get 7", log.Entries[0].Message);
            AreEqual("7 - all ok", log.Entries[0].Result);
        }
        #endregion

        #region With Inner Log

        [TestMethod]
        public void Func_InnerLog_Value()
        {
            var parentLog = new Log("tst.Test");
            var x = parentLog.Func(log =>
            {
                log.A("nice");
                return 7;
            });
            AreEqual(7, x);
            AreEqual($"{ThisMethodName()}()", parentLog.Entries[0].Message);
            AreEqual("7", parentLog.Entries[0].Result);
            AreEqual(3, parentLog.Entries.Count);
        }

        [TestMethod]
        public void Func_InnerLog_ParamAndValue()
        {
            var parentLog = new Log("tst.Test");
            var x = parentLog.Func("id: 7", log =>
            {
                log.A("nice");
                return 7;
            });
            AreEqual(7, x);
            AreEqual($"{ThisMethodName()}(id: 7)", parentLog.Entries[0].Message);
            AreEqual("7", parentLog.Entries[0].Result);
            AreEqual(3, parentLog.Entries.Count);
        }


        #endregion

        #region Inner Log with Result and Message

        [TestMethod]
        public void Func_InnerLog_ValueAndMessage()
        {
            var parentLog = new Log("tst.Test");
            var x = parentLog.Func(log =>
            {
                log.A("nice");
                return (7, "ok");
            });
            AreEqual(7, x);
            AreEqual($"{ThisMethodName()}()", parentLog.Entries[0].Message);
            AreEqual("7 - ok", parentLog.Entries[0].Result);
            AreEqual(3, parentLog.Entries.Count);
        }

        [TestMethod]
        public void Func_InnerLog_ParamAndValueAndMessage()
        {
            var parentLog = new Log("tst.Test");
            var x = parentLog.Func<int>("id: 7", log =>
            {
                log.A("nice");
                return (7, "ok");
            });
            AreEqual(7, x);
            AreEqual($"{ThisMethodName()}(id: 7)", parentLog.Entries[0].Message);
            AreEqual("7 - ok", parentLog.Entries[0].Result);
            AreEqual(3, parentLog.Entries.Count);
        }

        #endregion
    }
}
