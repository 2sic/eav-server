using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ToSic.Eav.Data.Builder
{
    public class DimensionBuilder
    {
        public Language Clone(ILanguage orig) => Clone(orig, null);

        public Language Clone(ILanguage orig, bool? readOnly) => new Language(orig.Key, readOnly ?? orig.ReadOnly, orig.DimensionId);

        public IImmutableList<ILanguage> Clone(IList<ILanguage> orig) => orig
            // 2023-02-24 2dm optimized this, keep comment till ca. 2023-04 in case something breaks
            //.Select(l => new Language { DimensionId = l.DimensionId, Key = l.Key } as ILanguage)
            .Select(l => Clone(l, null) as ILanguage)
            .ToImmutableList();

        // Note: can't be a new ImmutableArray<ILanguage>() because that causes problems!
        // Not sure why, but it complains that it can't tolist it
        public static IImmutableList<ILanguage> NoLanguages = new List<ILanguage>().ToImmutableList();// new ImmutableArray<ILanguage>();
    }
}
