using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Types
{
    public partial class Global
    {
        public static Log Log = new Log("Eav.GlbTyp");

        /// <summary>
        /// Dictionary of code-provided content-types, caches after first scan
        /// </summary>
        /// <returns></returns>
        public static ImmutableDictionary<string, IContentType> AllContentTypes()
        {
            if (_globalContentTypesCache != null) return _globalContentTypesCache;

            // copy the code-types dictionary...
            Log.Add($"AllContentTypes starting load at {DateTime.Now}");
            var codeTypes = new Dictionary<string, IContentType>(CodeContentTypes(Log), StringComparer.OrdinalIgnoreCase);

            // add runtime stuff
            var runtimeType = ContentTypesInRuntime(Log).ToList();

            // merge lists, preferences is code-types
            runtimeType.ForEach(t =>
            {
                if (!codeTypes.ContainsKey(t.StaticName))
                    codeTypes.Add(t.StaticName, t);
            });
            Log.Add($"will return {codeTypes.Count} content-types");
            // make sure it's case insensitive...
            _globalContentTypesCache = codeTypes.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
            return _globalContentTypesCache;
        }
        private static ImmutableDictionary<string, IContentType> _globalContentTypesCache;



        public static IContentType FindContentType(string name)
        {
            // use the types which have been loaded
            // this is to enable lookup of system types, while in the background we're still building the json-types
            // this is important, because the deserializer for json will also call this
            var types = _globalContentTypesCache != null ? AllContentTypes() : CodeContentTypes(Log);
            return types.ContainsKey(name) ? types[name] : null;
        }
    }
}
