#pragma warning disable xUnit1026
namespace ToSic.Lib.Core.Tests.LoggingTests;


// ReSharper disable once InconsistentNaming
public class Log_Do: LogTestBase
{
    [Fact] public void Do_Basic()
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do(() => { x = 7; });
        Do_Assert(ThisMethodName(), log, x, 7, 2);
    }

    [InlineData(true, 2)]
    [InlineData(false, 0)]
    [Theory]
    public void Do_Basic_Enabled(bool enabled, int expectedCount)
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do(() => { x = 7; }, enabled: enabled);
        Do_Assert(ThisMethodName(), log, x, 7, expectedCount);
    }

    private static void Do_Assert<T>(string fnName, Log log, T result, T expected, int expectedCount, string expectedSuffix = "()")
    {
        Equal(expected, result);
        Equal(expectedCount, log.Entries.Count); //, $"should have {expectedCount} entries (start/stop)");
        if (log.Entries.Any())
        {
            var header = log.Entries[0];
            Equal($"{fnName}{expectedSuffix}", header.Message);
        }
    }


    [Fact] public void Do_EnsureNullSafe()
    {
        var log = null as ILog;
        var x = 0;
        log.Do(() => { x = 7; });
        Equal(7, x);
    }

    [Fact] public void DoParameters_Basic()
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do("id: 7", () => { x = 7; });
        Do_Assert(ThisMethodName(), log, x, 7, 2, "(id: 7)");
    }

    [InlineData(true, 2)]
    [InlineData(false, 0)]
    [Theory]
    public void DoParameters_Basic_Enabled(bool enabled, int expectedCount)
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do("id: 7", () => { x = 7; }, enabled: enabled);
        Do_Assert(ThisMethodName(), log, x, 7, expectedCount, "(id: 7)");
    }

    [Fact]
    public void Do_WithInnerAddOnParent()
    {
        var log = new Log("tst.Test");
        var x = 0;
        log.Do(() =>
        {
            x = 7;
            log.A("nice");
        });
        Equal(7, x);
        Equal(3, log.Entries.Count);
    }

    [Fact] public void Do_WithInnerOnChild()
    {
        var parentLog = new Log("tst.Test");
        var x = 0;
        parentLog.Do(log =>
        {
            x = 7;
            log.A("nice");
        });
        Equal(7, x);
        Equal(3, parentLog.Entries.Count);
    }

    [Fact] public void Do_InDo_Parent()
    {
        var parentLog = new Log("tst.Test");
        var x = 0;
        parentLog.Do(() =>
        {
            x = 7;
            parentLog.Do(() => { x = 9; });
        });
        Equal(9, x);
        Equal(2 * 2, parentLog.Entries.Count);
    }

    [Fact] public void Do_InDo_Inner()
    {
        var parentLog = new Log("tst.Test");
        var x = 0;
        parentLog.Do(l =>
        {
            x = 7;
            l.Do(() => { x = 9; });
        });
        Equal(9, x);
        Equal(2 * 2, parentLog.Entries.Count);
    }

    [InlineData("no enabled", false, true, 2)]
    [InlineData("enabled true", true, true, 2)]
    [InlineData("enabled false", true, false, 0)]
    [Theory] public void DoAndReturnMessage_Basic(string name, bool testEnabled, bool enabled, int expectedCount)
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
        Equal(7, x);
        Equal(2, log.Entries.Count); // should have two entries (start/stop)
        Equal(resultMessage, log.Entries[0].Result);
    }

    [Fact] public void DoAndReturnMessage_WithInnerAdd()
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
        Equal(7, x);
        Equal(3, log.Entries.Count);
        Equal(resultMessage, log.Entries[0].Result);
    }

}