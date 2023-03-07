using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;

namespace ToSic.Testing.Shared
{
    public static class ValueBuilderTestExtensions
    {
        /// <summary>
        /// Test accessor to reduce use-count of the real code
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="languages"></param>
        /// <param name="fullEntityListForLookup"></param>
        /// <returns></returns>
        public static IValue Build4Test(this ValueBuilder vBuilder, ValueTypes type, object value, IList<ILanguage> languages,
            IEntitiesSource fullEntityListForLookup = null)
        {
            return vBuilder.Build(type, value, languages?.ToImmutableList(), fullEntityListForLookup);
        }
        public static IValue Build4Test(this ValueBuilder vBuilder, ValueTypes type, object value, IImmutableList<ILanguage> languages,
            IEntitiesSource fullEntityListForLookup = null)
        {
            return vBuilder.Build(type, value, languages, fullEntityListForLookup);
        }

    }
}
