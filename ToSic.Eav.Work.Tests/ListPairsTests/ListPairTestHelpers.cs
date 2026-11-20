using ToSic.Eav.Data.ListSlots.Sys;

namespace ToSic.Eav.ListPairsTests;

internal static class ListPairTestHelpers
{
    public const string PName = "Primary";
    public const string CName = "Coupled";

    public static Dictionary<string, List<int?>> CoupledIdLists(List<int?> primary, List<int?> coupled,
        string primaryField, string coupledField) =>
        new Dictionary<string, List<int?>>
        {

            {primaryField, primary},
            {coupledField, coupled }
        }.Stabilize(null!);

    internal static void AssertLength(Dictionary<string, List<int?>> list, int expectedLength)
    {
        var itemsNotWithLength = list.Values.Where(sublist => sublist.Count != expectedLength);
        Equal(0, itemsNotWithLength.Count());
    }

    internal static void AssertPositions(Dictionary<string, List<int?>> list, int position, int? primary, int? coupled)
    {
        var found = list.Values.First()[position];
        Equal(primary, found);//, $"Primary at {position} expected to be {primary} but was {found}");
        found = list.Values.Skip(1).First()[position];
        Equal(coupled, found);//, $"Primary at {position} expected to be {primary} but was {found}");
    }

}