using System.Linq;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavValues
    {
        /// <summary>
        /// Check if a language reference is read-only.
        /// </summary>
        public bool IsLanguageReadOnly(string language)
        {
            var languageReference = ToSicEavValuesDimensions
                .FirstOrDefault(reference => reference.Dimension.EnvironmentKey == language);
            return languageReference != null && languageReference.ReadOnly;
        }
    }
}
