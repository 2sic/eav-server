//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using ToSic.Eav.Data;
//using ToSic.Eav.Documentation;
//using ToSic.Eav.Logging;
//using ToSic.Eav.Run;

//namespace ToSic.Eav.Types
//{
//    [PrivateApi]
//    public class GlobalTypeLoader: HasLog<GlobalTypeLoader>
//    {
//        ///// <summary>
//        ///// Determines if the types have been fully loaded.
//        ///// This is important because during loading some code may already try to access some types, in which case it should
//        ///// be skipped. 
//        ///// </summary>
//        //public bool Ready = false;

//        public GlobalTypeLoader(IRuntime runtime) : base("Eav.GlTLdr", initialMessage: "Start Loading")
//        {
//            _runtime = runtime;
//        }
//        private readonly IRuntime _runtime;



//        public IEnumerable<IContentType> LoadTypes()
//        {
//            var wrapLog = Log.Call<IEnumerable<IContentType>>();
//            // copy the code-types dictionary...
//            // note 2019-01 2dm: as of now, no code-types are actually supported, I believe this 
//            // is a leftover of a temporary experiment
//            Log.Add($"AllContentTypes starting load at {DateTime.Now}");
//            var codeTypes = new Dictionary<string, IContentType>(StringComparer.InvariantCultureIgnoreCase);

//            // add runtime stuff
//            var runtimeType = ContentTypesInRuntime().ToList();

//            // merge lists, preferences is code-types
//            runtimeType.ForEach(t =>
//            {
//                if (!codeTypes.ContainsKey(t.StaticName))
//                    codeTypes.Add(t.StaticName, t);
//            });
//            Log.Add($"will return {codeTypes.Count} content-types");

//            // make sure it's case insensitive...
//            //ByStaticName = codeTypes.ToImmutableDictionary(StringComparer.InvariantCultureIgnoreCase);
//            //Ready = true;
//            return wrapLog("done", codeTypes.Values);
//        }

//        //internal ImmutableDictionary<string, IContentType> ByStaticName;

//        /// <summary>
//        /// All content-types available in Reflection; will cache after first scan
//        /// </summary>
//        /// <returns></returns>
//        private IEnumerable<IContentType> ContentTypesInRuntime()
//        {
//            Log.Add("ContentTypesInRuntime() loading");
//            var runtime = _runtime.Init(Log);
//            runtime?.LinkLog(Log);
//            return runtime?.LoadGlobalContentTypes() ?? new List<IContentType>();
//        }

//    }
//}
