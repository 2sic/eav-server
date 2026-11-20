using ToSic.Eav.Data.ListSlots.Sys;

namespace ToSic.Eav.ListPairsTests;

using static ListPairTestHelpers;

public class ListPairTestsReplace
{
    #region Test Constants - note that null-ints are actually stored as strings, otherwise we can't use them in the InlineData

    private const int LengthDef = 4;

    private const int Primary0Def = 1;
    private const int Primary1Def = 2;
    private const string? Primary2Def = null;
    private const int Primary3Def = 44;
    private const int Coupled0Def = 101;
    private const string? Coupled1Def = null;
    private const int Coupled2Def = 103;
    private const string? Coupled3Def = null;
    private const string? Coupled4Def = null;
    private const string? Coupled5Def = null;
    private const int PrimaryReplacement = 999;
    private const int CoupledReplacement = 777;

    #endregion

    #region Setup

    private static Dictionary<string, List<int?>> ReplaceAtX(int index, bool updatePair)
    {
        var pair = CoupledIdLists(
            [Primary0Def, Primary1Def, /*Primary2Def*/ null, Primary3Def],
            [Coupled0Def, /*Coupled1Def*/ null, Coupled2Def, /*Coupled3Def*/ null, /*Coupled4Def*/ null, /*Coupled5Def*/ null],
            PName,
            CName);
        var result = pair.ReplaceSlotTac(index, [
            (true, Values: PrimaryReplacement),
            (MayReplace: updatePair, Values: CoupledReplacement)
        ], null!);
        return result;
    }

    #endregion

    [Theory]
    // Within bounds, won't expand
    [InlineData(0, false, LengthDef)]
    [InlineData(0, true, LengthDef)]
    [InlineData(1, false, LengthDef)]
    [InlineData(1, true, LengthDef)]
    [InlineData(2, false, LengthDef)]
    [InlineData(2, true, LengthDef)]
    [InlineData(3, false, LengthDef)]
    [InlineData(3, true, LengthDef)]
    // Exceeding bounds, will expand
    [InlineData(LengthDef, false, LengthDef + 1)]
    [InlineData(LengthDef, true, LengthDef + 1)]
    [InlineData(9, true, 10)]
    [InlineData(9, false, 10)]
    public void ReplaceLengthCanExpand(int index, bool updatePair, int expectedLength)
        => AssertLength(ReplaceAtX(index, updatePair), expectedLength);


    [Theory]
    // Changed
    [InlineData(0, false, PrimaryReplacement, Coupled0Def, "Position 0 changed")]
    [InlineData(0, true, PrimaryReplacement, CoupledReplacement, "Position 0 changed")]
    // Unchanged
    [InlineData(1, false, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(1, true, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(2, false, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(2, true, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(3, false, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    [InlineData(3, true, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    public void ReplaceAt0(int index, bool updateBoth, int? expectedPrimary, int? expectedCoupled, string _)
        => AssertPositions(ReplaceAtX(0, updateBoth), index, expectedPrimary, expectedCoupled);


    [Theory]
    // Unchanged
    [InlineData(0, false, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    [InlineData(0, true, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    // Changed
    [InlineData(1, false, PrimaryReplacement, Primary2Def, "Position 1 unchanged")]
    [InlineData(1, true, PrimaryReplacement, CoupledReplacement, "Position 1 unchanged")]
    // Unchanged
    [InlineData(2, false, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(2, true, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(3, false, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    [InlineData(3, true, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    public void ReplaceAt1(int index, bool updateBoth, int? expectedPrimary, int? expectedCoupled, string _)
        => AssertPositions(ReplaceAtX(1, updateBoth), index, expectedPrimary, expectedCoupled);

    [Theory]
    // Unchanged
    [InlineData(0, false, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    [InlineData(0, true, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    [InlineData(1, false, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(1, true, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(2, false, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(2, true, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    // Changed
    [InlineData(3, false, PrimaryReplacement, Coupled3Def, "Position 3 unchanged")]
    [InlineData(3, true, PrimaryReplacement, CoupledReplacement, "Position 3 unchanged")]
    public void ReplaceAtEnd(int index, bool updateBoth, int? expectedPrimary, int? expectedCoupled, string _)
        => AssertPositions(ReplaceAtX(3, updateBoth), index, expectedPrimary, expectedCoupled);

    [Theory]
    // Unchanged
    [InlineData(0, false, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    [InlineData(0, true, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    [InlineData(1, false, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(1, true, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(2, false, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(2, true, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(3, false, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    [InlineData(3, true, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    // Changed
    [InlineData(4, false, PrimaryReplacement, Coupled4Def, "Position 4 changed")]
    [InlineData(4, true, PrimaryReplacement, CoupledReplacement, "Position 4 changed")]
    public void ReplaceAfterEnd(int index, bool updateBoth, int? expectedPrimary, int? expectedCoupled, string _)
        => AssertPositions(ReplaceAtX(4, updateBoth), index, expectedPrimary, expectedCoupled);


    [Theory]
    // Unchanged
    [InlineData(0, false, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    [InlineData(0, true, Primary0Def, Coupled0Def, "Position 0 unchanged")]
    [InlineData(1, false, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(1, true, Primary1Def, Primary2Def, "Position 1 unchanged")]
    [InlineData(2, false, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(2, true, Primary2Def, Coupled2Def, "Position 2 unchanged")]
    [InlineData(3, false, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    [InlineData(3, true, Primary3Def, Coupled3Def, "Position 3 unchanged")]
    // Changed
    [InlineData(9, false, PrimaryReplacement, Coupled4Def, "Position 9 changed")]
    [InlineData(9, true, PrimaryReplacement, CoupledReplacement, "Position 9 changed")]
    public void ReplacePastEnd(int index, bool updateBoth, int? expectedPrimary, int? expectedCoupled, string _)
        => AssertPositions(ReplaceAtX(9, updateBoth), index, expectedPrimary, expectedCoupled);

}
