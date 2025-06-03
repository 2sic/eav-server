using ToSic.Lib.Caching.PiggyBack;

namespace ToSic.Eav.Data;

partial record Entity : IHasPiggyBack
{
    [PrivateApi("WIP Piggyback")]
    public PiggyBack PiggyBack => field ??= new();
}