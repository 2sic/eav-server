﻿namespace ToSic.Eav.ListPairsTests;


public partial class ListPairTests
{
    public const string PName = "Primary";
    public const string CName = "Coupled";

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

    private static CoupledIdLists CoupledIdLists(List<int?> primary, List<int?> coupled,
        string primaryField, string coupledField)
    {
        return new(
            new()
            {
                {primaryField, primary},
                {coupledField, coupled }
            }, null);
    }

    private void AssertLength(CoupledIdLists pair, int expectedLength)
    {
        var itemsNotWithLength = pair.Lists.Values.Where(list => list.Count != expectedLength);
        Equal(0, itemsNotWithLength.Count());//, "lengths should now match");
        //Assert.Equal(expectedLength, pair.CoupledIds.Count, $"length was expected to be {expectedLength}");
    }

    private void AssertPositions(CoupledIdLists pair, int position, int? primary, int? coupled)
    {
        var found = pair.Lists.Values.First()[position];
        Equal(primary, found);//, $"Primary at {position} expected to be {primary} but was {found}");
        found = pair.Lists.Values.Skip(1).First()[position];
        Equal(coupled, found);//, $"Primary at {position} expected to be {primary} but was {found}");
    }

    private void AssertSequence(CoupledIdLists pair, int startIndex)
    {

    }

}