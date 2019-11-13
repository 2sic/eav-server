namespace ToSic.Eav.Data.Builder
{
    public static class DimensionBuilder
    {
        public static Language Copy(this Language orig, bool? readOnly = null, int? dimensionId = null, string key = null)
            => new Language
            {
                DimensionId = dimensionId ?? orig.DimensionId,
                ReadOnly = readOnly ?? orig.ReadOnly,
                Key = key ?? orig.Key
            };
    }
}
