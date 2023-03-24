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

        public object Get(string name, string noParamOrder = Eav.Parameters.Protector, string language = default, string[] languages = default) 
            => GetBestValue(name, HandleLanguageParams(language, languages));

        public TValue Get<TValue>(string name) => GetBestValue<TValue>(name, null);

        public TValue Get<TValue>(string name, string noParamOrder = Eav.Parameters.Protector, TValue fallback = default, string language = default, string[] languages = default) 
            => GetBestValue(name, HandleLanguageParams(language, languages)).ConvertOrFallback(fallback);

        private string[] HandleLanguageParams(string language, string[] languages) 
            => language.SafeAny() ? languages : language.HasValue() ? new[] { language } : null;
    }
}
