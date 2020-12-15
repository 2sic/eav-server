using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
#if NETFRAMEWORK
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use GetBestValue(name, languageList)")]
        [PrivateApi]
        public new object GetBestValue(string attributeName) => GetBestValue(attributeName, new string[0]);

        // 2020-10-30 trying to drop uses with ResolveHyperlinks
        ///// <inheritdoc />
        //public new TVal GetBestValue<TVal>(string name, bool resolveHyperlinks/* = false*/)
        //    => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        [PrivateApi("Testing / wip #IValueConverter")]
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
        public new TVal GetBestValue<TVal>(string name) => ChangeTypeOrDefault<TVal>(GetBestValue(name));


        // 2020-12-15 disabled - I believe it was never in use
        ///// <inheritdoc />
        //[PrivateApi("not sure yet if this is final - NEW")]
        //public object PrimaryValue(string attributeName)
        //    => GetBestValue(attributeName, new string[0]);

        ///// <inheritdoc />
        //[PrivateApi("not sure yet if this is final - NEW")]
        //public TVal PrimaryValue<TVal>(string attributeName)
        //    => GetBestValue<TVal>(attributeName, new string[0]);

#endif

    }
}
