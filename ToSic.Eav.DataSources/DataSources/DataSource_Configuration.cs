using System;
using ToSic.Lib.Documentation;
using static System.StringComparison;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        /// <summary>
        /// Correct prefix to use when retrieving a value from the current data sources configuration entity.
        /// Always use this variable, don't ever write the name yourself, as it could change in future.
        /// </summary>
        public static readonly string MyConfiguration = "MyConfiguration"; // WIP

        private static readonly string MyConfigOld = "Settings";

        /// <inheritdoc />
        public IDataSourceConfiguration Configuration => _config ?? (_config = new DataSourceConfiguration(Deps.ConfigDependencies, this));
        private IDataSourceConfiguration _config;

        /// <summary>
        /// Add a value to the configuration list for later resolving tokens and using in Cache-Keys.
        /// </summary>
        /// <param name="key">The internal key to reference this value in the Configuration[Key] dictionary.</param>
        /// <param name="token">The string containing [Tokens](xref:Abyss.Parts.LookUp.Tokens) which will be parsed to find the final value.</param>
        /// <param name="cacheRelevant">
        /// If this key should be part of the cache-key.
        /// Default is `true`.
        /// Set to `false` for parameters which don't affect the result or are very confidential (like passwords)
        /// </param>
        [PublicApi]
        protected void ConfigMask(string key, string token, bool cacheRelevant = true)
        {
            if (token.IndexOf($"{MyConfigOld}:", InvariantCultureIgnoreCase) > -1)
                throw new ArgumentException($"Don't user the source {MyConfigOld} for retrieving DS configuration any more (breaking change in v15). " +
                                            $"Instead, use the source name of the variable {nameof(MyConfiguration)}.");
            Configuration.Values.Add(key, token);
            if (cacheRelevant)
                CacheRelevantConfigurations.Add(key);
        }

        [PrivateApi("wip v15.03")]
        protected void ConfigMaskMyConfig(string key, string partialToken, bool cacheRelevant = true) 
            => ConfigMask(key, $"[{MyConfiguration}:{partialToken}]", cacheRelevant);

        /// <summary>
        /// Add a value to the configuration list for later resolving tokens and using in Cache-Keys.
        /// It will automatically generate a real token to retrieve the same named key from `MyConfiguration`
        /// </summary>
        /// <param name="keyAndToken">
        /// One of two options
        /// * Simple: just the name of the key which is the same as the token to retrieve, such as `MaxItems`
        /// * Advanced: The key with fallback or formatting, such as `MaxItem||100`
        /// </param>
        /// <param name="cacheRelevant">
        /// If this key should be part of the cache-key.
        /// Default is `true`.
        /// Set to `false` for parameters which don't affect the result or are very confidential (like passwords)
        /// </param>
        /// <remarks>
        /// * Added in v12.10
        /// * Published / documented it v15.03
        /// </remarks>
        [PublicApi]
        protected void ConfigMask(string keyAndToken, bool cacheRelevant = true)
        {
            // In case we have separators, then the internal lookup-key should only be the first part of this
            var separator = keyAndToken.IndexOfAny(new[] { '|' });
            var key = separator > 0 ? keyAndToken.Substring(0, separator) : keyAndToken;
            ConfigMaskMyConfig(key, keyAndToken, cacheRelevant);
        }
    }
}