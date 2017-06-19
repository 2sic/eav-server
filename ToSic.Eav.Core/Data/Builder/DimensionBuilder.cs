namespace ToSic.Eav.Data.Builder
{
    public static class DimensionBuilder
    {
        public static Dimension Copy(this Dimension orig, bool? readOnly = null, int? dimensionId = null, string key = null)
            => new Dimension
            {
                DimensionId = dimensionId ?? orig.DimensionId,
                ReadOnly = readOnly ?? orig.ReadOnly,
                Key = key ?? orig.Key
            };
    }
}
