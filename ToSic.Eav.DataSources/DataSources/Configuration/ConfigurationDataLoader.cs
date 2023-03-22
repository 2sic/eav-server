using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    [PrivateApi("Internal helper")]
    public class ConfigurationDataLoader: ServiceBase
    {
        #region Constructor / DI

        public ConfigurationDataLoader() : base(EavLogs.Eav + "CnfLdr")
        {
        }

        #endregion

        #region Get List From Cache or Generate

        public static ConcurrentDictionary<Type, List<ConfigMaskInfo>> Cache = new ConcurrentDictionary<Type, List<ConfigMaskInfo>>();

        public List<ConfigMaskInfo> GetTokens(Type type) => Log.Func(() =>
        {
            if (Cache.TryGetValue(type, out var cachedResult))
                return (cachedResult, "cached");

            var generateTokens = GenerateTokens(type);
            Cache[type] = generateTokens; // use indirection to make sure it's thread-safe, because Cache[type] could throw exception 'The given key was not present in dictionary'
            return (generateTokens, "generated");
        });

        #endregion

        public List<ConfigMaskInfo> GenerateTokens(Type type) => Log.Func(() =>
        {
            var configProps = type
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(ConfigurationAttribute), true))
                .Select(p =>
                {
                    // Important: this must go through Attribute.GetCustomAttribute, not p.GetCustomAttribute
                    // Otherwise inherited properties won't work
                    // see https://blog.seancarpenter.net/2012/12/15/getcustomattributes-and-overridden-properties/
                    var configAttr = Attribute.GetCustomAttributes(p, typeof(ConfigurationAttribute), true);
                    return new
                    {
                        Prop = p,
                        p.Name,
                        ConfigFrom = configAttr.FirstOrDefault() as ConfigurationAttribute
                    };
                })
                // Prevent errors if ever something fails to generate the attribute
                .Where(set => set.ConfigFrom != null)
                // Order by name, for consistent results in unit tests
                .OrderBy(set => set.Name)
                .ToList();

            var result = new List<ConfigMaskInfo>();
            foreach (var configProp in configProps)
            {
                var name = configProp.ConfigFrom.Field ?? configProp.Name;
                var fallback = $"{configProp.ConfigFrom.Fallback}";
                if (fallback.HasValue())
                    fallback = $"||{fallback}";
                var rule = $"{name}{fallback}";
                var token = $"[{DataSource.MyConfiguration}:{rule}]";
                result.Add(new ConfigMaskInfo
                {
                    Key = configProp.Name,
                    Token = token,
                    CacheRelevant = configProp.ConfigFrom.CacheRelevant
                });
            }

            return result;
        });
    }
}
