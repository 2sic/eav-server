using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Run;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Configuration
{
    public class Global
    {
        public const string GroupQuery = "query";
        public const string GroupConfiguration = "configuration";

        public static List<IEntity> List => _list ?? (_list = ConfigurationInRuntime());
        private static List<IEntity> _list;
        public static void Reset() => _list = null;


        /// <summary>
        /// Get the configuration for a specific type, or return null if no configuration found
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static IEntity For(string typeName) => List.FirstOrDefault(e => e.Type.Is(typeName));

        /// <summary>
        /// WIP / Experimental
        /// </summary>
        public static IEntity SystemSettings
        {
            get
            {
                var settings = List.FirstOrDefault(e => e.Type.Is("SystemSettings"));
                return settings;
            }
        }

        public static object Value(string typeName, string key) => For(typeName)?.Value(key);

        public static string GetOverride(string typeName, string key, string fallback)
        {
            var found = Value(typeName, key);
            return found?.ToString() ?? fallback;
        }

        /// <summary>
        /// All content-types available in Reflection; will cache after first scan
        /// </summary>
        /// <returns></returns>
        public static List<IEntity> ConfigurationInRuntime()
        {
            var log = new Log($"{LogNames.Eav}.Global");
            log.Add("Load Global Configurations");
            History.Add(Types.Global.LogHistoryGlobalTypes, log);
            var wrapLog = log.Call<List<IEntity>>();
            
            try
            {
                var runtime = Factory.StaticBuild<IRuntime>().Init(log);
                var list = runtime?.LoadGlobalItems(GroupConfiguration)?.ToList() ?? new List<IEntity>();
                return wrapLog($"{list.Count}", list);
            }
            catch (Exception e)
            {
                log.Exception(e);
                return wrapLog("error", new List<IEntity>());
            }
        }

    }
}
