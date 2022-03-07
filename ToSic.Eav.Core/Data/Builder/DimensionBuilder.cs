using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data.Builder
{
    public class DimensionBuilder
    {
        public Language Clone(ILanguage orig) => Clone(orig, null);

        public Language Clone(ILanguage orig, bool? readOnly) => new Language
        {
            DimensionId = orig.DimensionId,
            ReadOnly = readOnly ?? orig.ReadOnly,
            Key = orig.Key
        };

        public IList<ILanguage> Clone(IList<ILanguage> orig) => orig
            .Select(l => new Language { DimensionId = l.DimensionId, Key = l.Key } as ILanguage).ToList();

        public IList<ILanguage> NoLanguages() => new List<ILanguage>();

    }
}
