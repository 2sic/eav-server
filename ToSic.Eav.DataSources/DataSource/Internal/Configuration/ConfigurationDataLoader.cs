using System.Collections.Concurrent;

namespace ToSic.Eav.DataSource.Internal.Configuration;

[PrivateApi("Internal helper")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ConfigurationDataLoader() : ServiceBase(EavLogs.Eav + "CnfLdr")
{

    #region Get List From Cache or Generate

    internal static ConcurrentDictionary<Type, List<ConfigMaskInfo>> Cache = new();

    internal List<ConfigMaskInfo> GetTokens(Type type)
    {
        var l = Log.Fn<List<ConfigMaskInfo>>();
        if (Cache.TryGetValue(type, out var cachedResult))
            return l.Return(cachedResult, "cached");

        var generateTokens = GenerateTokens(type);
        Cache[type] = generateTokens; // use indirection to make sure it's thread-safe, because Cache[type] could throw exception 'The given key was not present in dictionary'
        return l.Return(generateTokens, "generated");
    }

    #endregion

    internal List<ConfigMaskInfo> GenerateTokens(Type type)
    {
        var l = Log.Fn<List<ConfigMaskInfo>>();
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
            var token = configProp.ConfigFrom.Token;
            if (token == null)
            {
                var name = configProp.ConfigFrom.Field ?? configProp.Name;
                var fallback = $"{configProp.ConfigFrom.Fallback}";
                // we mast use this logic instead of fallback.HasValue()
                // because of Csv example and \t for fallback value
                fallback = !string.IsNullOrEmpty(fallback) ? $"||{fallback}" : "";
                var rule = $"{name}{fallback}";
                token = $"[{DataSourceConstants.MyConfigurationSourceName}:{rule}]";
            }

            result.Add(new()
            {
                Key = configProp.Name,
                Token = token,
                CacheRelevant = configProp.ConfigFrom.CacheRelevant
            });
        }

        return l.ReturnAsOk(result);
    }
}