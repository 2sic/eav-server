using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Types
{
    public class Global
    {
        public static ILog Log = new Log("Eav.GlbTyp");

        /// <summary>
        /// The constructor is automatically run exactly 1x when the first bit of code
        /// accesses any static property of this object
        /// </summary>
       static Global() => History.Add("global-types", Log);

        /// <summary>
        /// Now using a Lazy method like recommended in
        /// http://csharpindepth.com/Articles/General/Singleton.aspx#lock
        /// </summary>
        private static readonly Lazy<GlobalTypeLoader> Lazy 
            = new Lazy<GlobalTypeLoader>(() => new GlobalTypeLoader(Log));

        /// <summary>
        /// Dictionary of code-provided content-types, caches after first scan
        /// </summary>
        /// <returns></returns>
        public static ImmutableDictionary<string, IContentType> AllContentTypes() 
            => Lazy.Value.ByStaticName;


        public static IContentType FindContentType(string name)
        {
            // use the types which have been loaded
            // this is to enable lookup of system types, while in the background we're still building the json-types
            // this is important, because the deserializer for json will also call this
            // when trying to load the first file-system based content-types (while initializing the types)
            if (!Lazy.IsValueCreated)
            {
                Log.Add($"FindContentType({name}) before global types have been loaded = null");
                return null;
            }

            var types = Lazy.Value.ByStaticName;
            if (types.ContainsKey(name))
                return types[name];

            // now also try with nice-name
            var niceNamedType = Lazy.Value.ByNiceName;
            return niceNamedType.ContainsKey(name) 
                ? niceNamedType[name]
                : null;
        }
    }
}
