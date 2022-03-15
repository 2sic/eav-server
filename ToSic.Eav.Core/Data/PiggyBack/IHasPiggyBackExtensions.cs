using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data.PiggyBack
{
    [PrivateApi]
    // ReSharper disable once InconsistentNaming
    public static class IHasPiggyBackExtensions
    {

        [PrivateApi]
        public static TData GetPiggyBack<TData>(this IHasPiggyBack parent, string key, Func<TData> create) => parent.PiggyBack.GetOrGenerate(key, create);

    }
}
