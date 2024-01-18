using ToSic.Eav.Data.PiggyBack;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

partial class Entity: IHasPiggyBack
{
    [PrivateApi("WIP Piggyback")]
    public PiggyBack.PiggyBack PiggyBack => _piggyBack ??= new();
    private PiggyBack.PiggyBack _piggyBack;
}