using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Configuration
{
    public class Global
    {
        public static List<IEntity> List => _list ?? (_list = ConfigurationInRuntime());
        private static List<IEntity> _list;


        /// <summary>
        /// Get the configuration for a specific type, or return null if no configuration found
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static IEntity For(string typeName) =>
            List.FirstOrDefault(e => e.Type.StaticName == typeName || e.Type.Name == typeName);

        public static object Value(string typeName, string key) => For(typeName)?.GetBestValue(key);

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
            List<IEntity> list;
            try
            {
                var runtime = Factory.Resolve<IRuntime>();
                list = runtime?.LoadGlobalItems("configuration")?.ToList() ?? new List<IEntity>();
            }
            catch
            {
                list = new List<IEntity>();
            }
            return list;
        }
    }
}
