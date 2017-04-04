using System.Linq;

namespace ToSic.Eav
{
    public partial class EavValue
    {
        /// <summary>
        /// Check if a language reference is read-only.
        /// </summary>
        internal bool IsLanguageReadOnly(string language)
        {
            var languageReference = ValuesDimensions
                .FirstOrDefault(reference => reference.Dimension.ExternalKey == language);
            return languageReference != null && languageReference.ReadOnly;
        }
    }
}
