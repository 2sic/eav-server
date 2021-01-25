namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public object Value(string field) => GetBestValue(field, null);

        /// <inheritdoc />
        public T Value<T>(string field) => ChangeTypeOrDefault<T>(GetBestValue(field, null));

    }
}
