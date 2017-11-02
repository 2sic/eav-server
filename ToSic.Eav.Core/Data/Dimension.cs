using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a Dimension Assignment
    /// </summary>
    public class Dimension : ILanguage
    {
        public int DimensionId { get; set; }
        public bool ReadOnly { get; set; }

        public string Key
        {
            get => _key;
            set => _key = value.ToLowerInvariant();
        }
        private string _key;
    }
}