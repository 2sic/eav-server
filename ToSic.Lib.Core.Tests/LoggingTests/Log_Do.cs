using ToSic.Lib.Logging;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests;

[TestClass]
// ReSharper disable once InconsistentNaming
public class Log_Do: LogTestBase
{
    [TestMethod] public void Do_Basic()
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do(() => { x = 7; });
        Do_Assert(ThisMethodName(), log, x, 7, 2);
    }

    [DataRow(true, 2)]
    [DataRow(false, 0)]
    [TestMethod] public void Do_Basic_Enabled(bool enabled, int expectedCount)
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do(() => { x = 7; }, enabled: enabled);
        Do_Assert(ThisMethodName(), log, x, 7, expectedCount);
    }

    private static void Do_Assert<T>(string fnName, Log log, T result, T expected, int expectedCount, string expectedSuffix = "()")
    {
        AreEqual(expected, result);
        AreEqual(expectedCount, log.Entries.Count, $"should have {expectedCount} entries (start/stop)");
        if (log.Entries.Any())
        {
            var header = log.Entries[0];
            AreEqual($"{fnName}{expectedSuffix}", header.Message);
        }
    }


    [TestMethod] public void Do_EnsureNullSafe()
    {
        var log = null as ILog;
        var x = 0;
        log.Do(() => { x = 7; });
        AreEqual(7, x);
    }

    [TestMethod] public void DoParameters_Basic()
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do("id: 7", () => { x = 7; });
        Do_Assert(ThisMethodName(), log, x, 7, 2, "(id: 7)");
    }

    [DataRow(true, 2)]
    [DataRow(false, 0)]
    [TestMethod] public void DoParameters_Basic_Enabled(bool enabled, int expectedCount)
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do("id: 7", () => { x = 7; }, enabled: enabled);
        Do_Assert(ThisMethodName(), log, x, 7, expectedCount, "(id: 7)");
    }

    [TestMethod] public void Do_WithInnerAddOnParent()
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

    [TestMethod] public void Do_WithInnerOnChild()
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

    [TestMethod] public void Do_InDo_Parent()
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

    [TestMethod] public void Do_InDo_Inner()
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

    [DataRow("no enabled", false, true, 2)]
    [DataRow("enabled true", true, true, 2)]
    [DataRow("enabled false", true, false, 0)]
    [TestMethod] public void DoAndReturnMessage_Basic(string name, bool testEnabled, bool enabled, int expectedCount)
    {
        var resultMessage = "this is ok";
        var log = new Log("tst.Test");
        var x = 0;
        if (testEnabled)
            log.Do(() =>
            {
                x = 7;
                return resultMessage;
            });
        else
            log.Do(() =>
            {
                x = 7;
                return resultMessage;
            }, enabled: enabled);
        AreEqual(7, x);
        AreEqual(2, log.Entries.Count, "should have two entries (start/stop)");
        AreEqual(resultMessage, log.Entries[0].Result);
    }

    [TestMethod] public void DoAndReturnMessage_WithInnerAdd()
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