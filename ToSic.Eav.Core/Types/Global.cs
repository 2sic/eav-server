using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Types
{
    public partial class Global
    {
        /// <summary>
        /// Dictionary of code-provided content-types, caches after first scan
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, IContentType> AllContentTypes()
        {
            if (_globalContentTypesCache != null) return _globalContentTypesCache;

            // copy the code-types dictionary...
            var codeTypes = new Dictionary<string, IContentType>(CodeContentTypes(), StringComparer.OrdinalIgnoreCase);

            // add runtime stuff
            var runtimeType = ContentTypesInRuntime().ToList();

            // merge lists, preferences is code-types
            runtimeType.ForEach(t =>
            {
                if (!codeTypes.ContainsKey(t.StaticName))
                    codeTypes.Add(t.StaticName, t);
            });

            // make sure it's case insensitive...
            _globalContentTypesCache = codeTypes;
            return _globalContentTypesCache;
        }
        private static Dictionary<string, IContentType> _globalContentTypesCache;



        public static IContentType FindContentType(string name)
        {
            // use the types which have been loaded
            // this is to enable lookup of system types, while in the background we're still building the json-types
            // this is important, because the deserializer for json will also call this
            var types = _globalContentTypesCache != null ? AllContentTypes() : CodeContentTypes();
            return types.ContainsKey(name) ? types[name] : null;
        }
    }
}
