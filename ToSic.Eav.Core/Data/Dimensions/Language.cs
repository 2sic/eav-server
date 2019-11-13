using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a Dimension / Language Assignment
    /// </summary>
    [PublicApi]
    public class Language : ILanguage
    {
        /// <inheritdoc />
        public int DimensionId { get; set; }

        /// <inheritdoc />
        public bool ReadOnly { get; set; }

        /// <inheritdoc />
        public string Key
        {
            get => _key;
            set => _key = value.ToLowerInvariant();
        }
        private string _key;
    }
}