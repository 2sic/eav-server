using ToSic.Eav.Data.PiggyBack;

namespace ToSic.Eav.Data;

partial record Entity : IHasPiggyBack
{
    [PrivateApi("WIP Piggyback")]
    public PiggyBack.PiggyBack PiggyBack => field ??= new();
}