using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Types
{
    public class GlobalTypes: HasLog
    {
        public static string LogHistoryGlobalTypes = "global-types";

        public GlobalTypes(LogHistory logHistory, GlobalTypeLoader typeLoader): base($"{LogNames.Eav}.GlbTyp")
        {
            logHistory.Add(LogHistoryGlobalTypes, Log);
            if (TypeLoader == null)
                TypeLoader = typeLoader;
            else
                Log.Add(
                    $"{nameof(GlobalTypes)} was created again, which is not expected since it's a singleton. " +
                    "But can happen in edge cases like when other DLLs re-init the dependency injection. " +
                    "As of now, we'll ignore additional initializations since the internal data is static.");
        }

        /// <summary>
        /// Careful: this initially doesn't exist which is by design
        /// WIP: 2021-10-28 trying to fix edge case where DI was re-used
        /// </summary>
        public static GlobalTypeLoader TypeLoader;

        /// <summary>
        /// This starts the loading process - this should only ever be called once!
        /// </summary>
        internal void StartUp(ILog targetLogger) => TypeLoader.Init(targetLogger).BuildCache();

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
            if (TypeLoader == null || !TypeLoader.Ready)
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
