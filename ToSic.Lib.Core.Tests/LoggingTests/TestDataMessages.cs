using static ToSic.Lib.Core.Tests.LoggingTests.LogTestBase;

namespace ToSic.Lib.Core.Tests.LoggingTests;

internal class TestDataMessages
{
    public static IEnumerable<object?[]> SimpleMessages(int depth) => new[]
    {
        new object?[] { "Basic", "message", "message", ResultNone, depth },
        new object?[] { "Basic 2", "This Is A Test", "This Is A Test", ResultNone, depth },
    };

}