using ToSic.Eav.Data.ListSlots.Sys;

namespace ToSic.Eav.ListPairsTests;

using static ListPairTestHelpers;

public class ListPairTests
{

    [Fact]
    public void UnmatchedPairsAutoFixShort()
    {
        var pair = new CoupledIdLists(
            new()
            {
                {PName, [1, 2, null, 3] },
                {CName, [null, null] }
            }, null);
        AssertLength(pair, 4);
    }

    [Fact]
    public void UnmatchedPairsAutoFixLong()
    {
        var pair = CoupledIdLists([1, 2, null, 3],
            [null, null, null, null, null, null],
            PName,CName);
        AssertLength(pair, 4);
    }
}
