namespace ToSic.Sys.Caching.PiggyBack;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasPiggyBack
{
    PiggyBack PiggyBack { get; }
}