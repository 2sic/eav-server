using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public object Value(string field) => GetBestValue(field, null);

        /// <inheritdoc />
        public T Value<T>(string field) => GetBestValue<T>(field, null);

        public object Get(string name) => GetBestValue(name, null);
        public T Get<T>(string name) => GetBestValue<T>(name, null);

        public T Get<T>(string name, string noParamOrder = Eav.Parameters.Protector, T fallback = default)
            => GetBestValue(name, null).ConvertOrFallback(fallback);

    }
}
