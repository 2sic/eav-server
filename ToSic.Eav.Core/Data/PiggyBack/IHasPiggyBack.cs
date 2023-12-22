using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.PiggyBack;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IHasPiggyBack
{
    PiggyBack PiggyBack { get; }
}