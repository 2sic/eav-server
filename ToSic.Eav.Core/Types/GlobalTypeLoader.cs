using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Types
{
    internal class GlobalTypeLoader: HasLog
    {
        public GlobalTypeLoader(ILog parentLog) 
            : base("Eav.GlTLdr", parentLog, "Start Loading")
        {
            BuildCache();
        }


        private void BuildCache()
        {
            // copy the code-types dictionary...
            // note 2019-01 2dm: as of now, no code-types are actually supported, I believe this 
            // is a leftover of a temporary experiment
            Log.Add($"AllContentTypes starting load at {DateTime.Now}");
            var codeTypes =
                new Dictionary<string, IContentType>(ReflectionTypes.FakeCache, StringComparer.OrdinalIgnoreCase);

            // add runtime stuff
            var runtimeType = ContentTypesInRuntime().ToList();

            // merge lists, preferences is code-types
            runtimeType.ForEach(t =>
            {
                if (!codeTypes.ContainsKey(t.StaticName))
                    codeTypes.Add(t.StaticName, t);
            });
            Log.Add($"will return {codeTypes.Count} content-types");

            // create the nice-names dictionary, so it always exists when the static-name dic exists
            ByNiceName = codeTypes
                .ToImmutableDictionary(t => t.Value.Name, t => t.Value, StringComparer.OrdinalIgnoreCase);

            // make sure it's case insensitive...
            ByStaticName = codeTypes.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        }

        internal ImmutableDictionary<string, IContentType> ByStaticName;
        internal ImmutableDictionary<string, IContentType> ByNiceName;

        /// <summary>
        /// All content-types available in Reflection; will cache after first scan
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IContentType> ContentTypesInRuntime()
        {
            Log.Add("ContentTypesInRuntime() loading");
            var runtime = Factory.Resolve<IRuntime>();
            runtime?.LinkLog(Log);
            return runtime?.LoadGlobalContentTypes() ?? new List<IContentType>();
        }

    }
}
