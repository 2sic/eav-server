using System;
using System.Collections.Generic;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Contains all the common constants etc. for Data-Scopes
    /// </summary>
    public class Scopes
    {
        /// <summary>
        /// App Scope for App stuff
        /// </summary>
        public static string App = "System.App";
        private const string AppOld = "2SexyContent-App";

        public static readonly string DataSources = "System.DataSources";

        public static readonly string Decorators = "System.Decorators";

        /// <summary>
        /// The `Default` scope contains Content-Types which are usually used in this App
        /// </summary>
        public static readonly string Default = "Default";
        private const string DefaultOld = "2SexyContent";


        public static readonly string Fields = "System.Fields";

        /// <summary>
        /// The `System` scope is meant for very internal Content-Types
        /// </summary>
        public static readonly string System = "System";

        public static readonly string SystemConfiguration = "System.Configuration";


        /// <summary>
        /// Anything related to Content Management functionality, like Content-Block
        /// </summary>
        public static readonly string Cms = "System.Cms";
        private const string CmsOld = "2SexyContent-System";

        [PrivateApi]
        public static Dictionary<string, string> ScopesWithNames = new(StringComparer.InvariantCultureIgnoreCase)
        {
            {Default, Default},
            {SystemConfiguration, "Configuration (Views etc.)" },
            {Cms, "System: CMS"},
            {App, "System: App"},
            {System, "System: System"},
            {DataSources, "System: DataSources"},
            {Decorators, "System: Decorators"},
            {Fields, "System: Fields"}
        };



        [PrivateApi]
        public static string RenameOldScope(string scope)
        {
            if (!scope.HasValue()) return scope;
            if (DefaultOld.EqualsInsensitive(scope)) return Default;
            if (AppOld.EqualsInsensitive(scope)) return App;
            if (CmsOld.EqualsInsensitive(scope)) return Cms;
            return scope;
        }
    }
}
