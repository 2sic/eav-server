using ToSic.Eav.Data.PiggyBack;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

public partial class Entity: IHasPiggyBack
{
    [PrivateApi("WIP Piggyback")]
    public PiggyBack.PiggyBack PiggyBack => _piggyBack ??= new PiggyBack.PiggyBack();
    private PiggyBack.PiggyBack _piggyBack;
}