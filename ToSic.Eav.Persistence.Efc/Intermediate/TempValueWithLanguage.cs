using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence.Efc.Intermediate
{
    internal class TempValueWithLanguage
    {
        public string Value;
        public IImmutableList<ILanguage> Languages;

    }
}
