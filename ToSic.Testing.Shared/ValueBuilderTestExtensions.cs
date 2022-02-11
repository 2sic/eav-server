using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;

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
        public static IValue ValueBuilderBuildTest(ValueTypes type, object value, List<ILanguage> languages,
            IEntitiesSource fullEntityListForLookup = null)
        {
            return ValueBuilder.Build(type, value, languages, fullEntityListForLookup);
        }

    }
}
