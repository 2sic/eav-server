using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data.PiggyBack
{
    [PrivateApi]
    public interface IHasPiggyBack
    {
        PiggyBack PiggyBack { get; }
    }
}
