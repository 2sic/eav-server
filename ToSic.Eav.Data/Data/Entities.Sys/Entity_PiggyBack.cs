using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Data.Entities.Sys;

partial record Entity : IHasPiggyBack
{
    [PrivateApi("WIP Piggyback")]
    public PiggyBack PiggyBack => field ??= new();
}