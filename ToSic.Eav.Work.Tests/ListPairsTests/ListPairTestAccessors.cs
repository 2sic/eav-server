using ToSic.Eav.Data.ListSlots.Sys;

namespace ToSic.Eav.ListPairsTests;

internal static class ListPairTestAccessors
{
    public static Dictionary<string, List<int?>> AddSlotTac(this Dictionary<string, List<int?>> lists, int? index, int?[] ids, ILog? log)
        => lists.AddSlot(index, ids, log);
    public static Dictionary<string, List<int?>> MoveSlotTac(this Dictionary<string, List<int?>> lists, int sourceIndex, int targetIndex, ILog? log)
        => lists.MoveSlot(sourceIndex, targetIndex, log);

    public static Dictionary<string, List<int?>> ReorderSlotsTac(this Dictionary<string, List<int?>> original, int[] newSequence, ILog? log)
        => original.ReorderSlots(newSequence, log);

    public static Dictionary<string, List<int?>> ReplaceSlotTac(this Dictionary<string, List<int?>> original, int index, (bool MayReplace, int? Values)[] values, ILog? log)
        => original.ReplaceSlot(index, values, log);

    public static Dictionary<string, List<int?>> RemoveSlotsTac(this Dictionary<string, List<int?>> original, int index, ILog? log)
        => original.RemoveSlots(index, log);
}
