using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class Global
    {
        public const string GroupQuery = "query";
        public const string GroupConfiguration = "configuration";

        private const int ContentTypeZeroMaker = 10000000;
        public const int GlobalContentTypeMin = int.MaxValue / ContentTypeZeroMaker * ContentTypeZeroMaker;
        public const int GlobalContentTypeSourceSkip = 1000;

        /// <summary>
        /// The list is set by the ConfigurationLoader. That must run at application start.
        /// </summary>
        public static List<IEntity> List { get; internal set; }

        /// <summary>
        /// Get the configuration for a specific type, or return null if no configuration found
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static IEntity For(string typeName) => List.FirstOrDefault(e => e.Type.Is(typeName));

        /// <summary>
        /// WIP / Experimental
        /// </summary>
        public static IEntity SystemSettings => List.FirstOrDefault(e => e.Type.Is(ConfigurationConstants.Settings.SystemType));

        public static IEntity SystemResources => List.FirstOrDefault(e => e.Type.Is(ConfigurationConstants.Resources.SystemType));

        public static object Value(string typeName, string key) => For(typeName)?.Value(key);

        public static string GetOverride(string typeName, string key, string fallback)
        {
            var found = Value(typeName, key);
            return found?.ToString() ?? fallback;
        }
    }
}
