using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Data.Sys.Entities;

partial record Entity : IHasPiggyBack
{
    [PrivateApi("WIP Piggyback")]
    [field: AllowNull, MaybeNull]
    public PiggyBack PiggyBack => field ??= new();
}