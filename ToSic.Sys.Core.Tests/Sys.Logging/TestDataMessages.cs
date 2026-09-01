using static ToSic.Sys.Logging.LogTestBase;

namespace ToSic.Sys.Logging;

internal class TestDataMessages
{
    public static IEnumerable<object?[]> SimpleMessages(int depth) =>
    [
        ["Basic", "message", "message", ResultNone, depth],
        ["Basic 2", "This Is A Test", "This Is A Test", ResultNone, depth]
    ];

}