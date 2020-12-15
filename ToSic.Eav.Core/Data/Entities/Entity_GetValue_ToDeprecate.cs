using ToSic.Eav.Context;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        [PrivateApi("Testing / wip #IValueConverter")]
        public new object GetBestValue(string attributeName) => GetBestValue(attributeName, new string[0]);


        // 2020-10-30 trying to drop uses with ResolveHyperlinks
        ///// <inheritdoc />
        //public new TVal GetBestValue<TVal>(string name, bool resolveHyperlinks/* = false*/)
        //    => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        [PrivateApi("Testing / wip #IValueConverter")]
        public new TVal GetBestValue<TVal>(string name) => ChangeTypeOrDefault<TVal>(GetBestValue(name));


        /// <inheritdoc />
        [PrivateApi("not sure yet if this is final - NEW")]
        public object PrimaryValue(string attributeName)
            => GetBestValue(attributeName, new string[0]);

        /// <inheritdoc />
        [PrivateApi("not sure yet if this is final - NEW")]
        public TVal PrimaryValue<TVal>(string attributeName)
            => GetBestValue<TVal>(attributeName, new string[0]);


        /// <inheritdoc />
        [PrivateApi("don't publish yet, not really final")]
        public object Value(string field)
            => GetBestValue(field, new[] { IZoneCultureResolverExtensions.ThreadCultureNameNotGood() });

        /// <inheritdoc />
        [PrivateApi("don't publish yet, not really final")]
        public T Value<T>(string field)
            => ChangeTypeOrDefault<T>(GetBestValue(field, new[] { IZoneCultureResolverExtensions.ThreadCultureNameNotGood() }));
    }
}
