using System.Linq;

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
                .FirstOrDefault(reference => reference.Dimension.ExternalKey == language);
            return languageReference != null && languageReference.ReadOnly;
        }
    }
}
