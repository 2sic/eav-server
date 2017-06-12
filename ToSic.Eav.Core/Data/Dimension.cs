using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Dimension Assignment
    /// </summary>
    public class Dimension : IDimension, ILanguage
    {
        public int DimensionId { get; set; }
        public bool ReadOnly { get; set; }
        public string Key { get; set; }
    }
}