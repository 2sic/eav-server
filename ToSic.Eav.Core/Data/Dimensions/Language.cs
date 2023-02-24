using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a Dimension / Language Assignment
    /// </summary>
    [PrivateApi("2021-09-30 hidden, previously marked as PublicApi_Stable_ForUseInYourCode")]
    public class Language : ILanguage
    {
        public Language(string key, bool readOnly, int dimensionId = 0)
        {
            Key = key.ToLowerInvariant();
            ReadOnly = readOnly;
            DimensionId = dimensionId;
        }

        /// <inheritdoc />
        public int DimensionId { get; }

        /// <inheritdoc />
        public bool ReadOnly { get; }

        /// <inheritdoc />
        public string Key { get; }
    }
}