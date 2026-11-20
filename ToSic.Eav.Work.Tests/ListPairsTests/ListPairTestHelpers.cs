using ToSic.Eav.Data.ListSlots.Sys;

namespace ToSic.Eav.ListPairsTests;

internal static class ListPairTestHelpers
{
    public const string PName = "Primary";
    public const string CName = "Coupled";

    public static CoupledIdLists CoupledIdLists(List<int?> primary, List<int?> coupled,
        string primaryField, string coupledField)
    {
        return new(
            new()
            {
                {primaryField, primary},
                {coupledField, coupled }
            }, null);
    }

    internal static void AssertLength(CoupledIdLists pair, int expectedLength)
    {
        var itemsNotWithLength = pair.Lists.Values.Where(list => list.Count != expectedLength);
        Equal(0, itemsNotWithLength.Count());//, "lengths should now match");
        //Assert.Equal(expectedLength, pair.CoupledIds.Count, $"length was expected to be {expectedLength}");
    }

    internal static void AssertPositions(CoupledIdLists pair, int position, int? primary, int? coupled)
    {
        var found = pair.Lists.Values.First()[position];
        Equal(primary, found);//, $"Primary at {position} expected to be {primary} but was {found}");
        found = pair.Lists.Values.Skip(1).First()[position];
        Equal(coupled, found);//, $"Primary at {position} expected to be {primary} but was {found}");
    }

}