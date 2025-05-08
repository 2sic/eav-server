namespace ToSic.Eav.Data.PiggyBack;

[PrivateApi]
// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class IHasPiggyBackExtensions
{

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static TData GetPiggyBack<TData>(this IHasPiggyBack parent, string key, Func<TData> create) 
        => parent.PiggyBack.GetOrGenerate(key, create);

}