namespace ToSic.Eav.Data.PiggyBack;

[PrivateApi]
// ReSharper disable once InconsistentNaming
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class IHasPiggyBackExtensions
{

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TData GetPiggyBack<TData>(this IHasPiggyBack parent, string key, Func<TData> create) 
        => parent.PiggyBack.GetOrGenerate(key, create);

}