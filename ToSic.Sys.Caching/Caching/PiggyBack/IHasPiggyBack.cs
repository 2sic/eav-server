namespace ToSic.Eav.Data.PiggyBack;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasPiggyBack
{
    PiggyBack PiggyBack { get; }
}