using ToSic.Eav.DataSource.Sys.Configuration;

namespace ToSic.Eav.DataSource.Sys;

public static class DataSourceConfigMasks
{
    /// <summary>
    /// Load all [Configuration] attributes and ensure we have the config masks.
    /// </summary>
    internal static void AutoLoadAllConfigMasks(this DataSourceBase dataSource, Type dataSourceType, ConfigurationDataLoader configDataLoader)
    {
        // Figure out the type which provides the configuration
        // This was WIP since v13 and as of 2026-02 does not seem to be used anywhere.
        // Disabled for now...
        //var redefined = Attribute
        //    .GetCustomAttributes(dataSourceType, typeof(ConfigurationSpecsWipAttribute), true)
        //    .FirstOrDefault() as ConfigurationSpecsWipAttribute;

        var type = /*redefined?.SpecsType ??*/ dataSourceType;

        // Load all config masks which are defined on attributes
        var configMasks = configDataLoader.GetTokens(type);
        configMasks.ForEach(cm => dataSource.ConfigMask(cm.Key, cm.Token, cm.CacheRelevant));
    }


    /// <summary>
    /// Add a value to the configuration list for later resolving tokens and using in Cache-Keys.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="key">The internal key to reference this value in the Configuration[Key] dictionary.</param>
    /// <param name="token">The string containing [Tokens](xref:Abyss.Parts.LookUp.Tokens) which will be parsed to find the final value.</param>
    /// <param name="cacheRelevant">
    /// If this key should be part of the cache-key.
    /// Default is `true`.
    /// Set to `false` for parameters which don't affect the result or are very confidential (like passwords)
    /// </param>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    internal static void ConfigMask(this DataSourceBase dataSource, string key, string token, bool cacheRelevant = true)
    {
        const string myConfigOld = "Settings";
        if (token.IndexOf($"[{myConfigOld}:", StringComparison.InvariantCultureIgnoreCase) > -1)
            throw new ArgumentException($"Don't user the source {myConfigOld} for retrieving DS configuration any more (breaking change in v15). " +
                                        $"Instead, use the source name of the variable {nameof(DataSourceConstants.MyConfigurationSourceName)}.");
        // New v15.04 - only add if it has not already been set
        // This is to ensure that config masks don't overwrite data which 
        ((DataSourceConfiguration)dataSource.Configuration).AddIfMissing(key, token);
        if (cacheRelevant)
            dataSource.CacheRelevantConfigurations.Add(key);
    }

}