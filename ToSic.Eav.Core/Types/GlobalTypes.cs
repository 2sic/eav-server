using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Types
{
    public class GlobalTypes: HasLog
    {
        public static string LogHistoryGlobalTypes = "global-types";

        public GlobalTypes(LogHistory logHistory): base($"{LogNames.Eav}.GlbTyp")
        {
            logHistory.Add(LogHistoryGlobalTypes, Log);
        }

        /// <summary>
        /// Careful: this initially doesn't exist which is by design
        /// </summary>
        internal static GlobalTypeLoader TypeLoader;

        /// <summary>
        /// Dictionary of code-provided content-types, caches after first scan
        /// </summary>
        /// <returns></returns>
        public ImmutableDictionary<string, IContentType> AllContentTypes() => TypeLoader.ByStaticName;

        public IContentType FindContentType(string name)
        {
            // use the types which have been loaded
            // this is to enable lookup of system types, while in the background we're still building the json-types
            // this is important, because the deserializer for json will also call this
            // when trying to load the first file-system based content-types (while initializing the types)
            if (TypeLoader == null)
            {
                Log.Add($"FindContentType({name}) used before global types have been loaded = null");
                return null;
            }

            if (TypeLoader.ByStaticName.ContainsKey(name))
                return TypeLoader.ByStaticName[name];

            // now also try with nice-name
            return TypeLoader.ByNiceName.ContainsKey(name)
                ? TypeLoader.ByNiceName[name]
                : null;
        }

    }
}
