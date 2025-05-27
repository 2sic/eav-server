using ToSic.Lib.Caching.PiggyBack;

namespace ToSic.Eav.Data;

partial record Entity : IHasPiggyBack
{
    [PrivateApi("WIP Piggyback")]
    public Lib.Caching.PiggyBack.PiggyBack PiggyBack => field ??= new();
}