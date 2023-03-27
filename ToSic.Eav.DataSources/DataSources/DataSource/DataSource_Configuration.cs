using System;
using ToSic.Eav.DataSource;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static System.StringComparison;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        /// <summary>
        /// Correct prefix to use when retrieving a value from the current data sources configuration entity.
        /// Always use this variable, don't ever write the name as a string, as it could change in future.
        /// </summary>
        public static readonly string MyConfiguration = "MyConfiguration";

        private static readonly string MyConfigOld = "Settings";

        /// <inheritdoc />
        public IDataSourceConfiguration Configuration => _config.Get(() => base.Services.Configuration.Attach(this));
        private readonly GetOnce<IDataSourceConfiguration> _config = new GetOnce<IDataSourceConfiguration>();

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
            if (token.IndexOf($"[{MyConfigOld}:", InvariantCultureIgnoreCase) > -1)
                throw new ArgumentException($"Don't user the source {MyConfigOld} for retrieving DS configuration any more (breaking change in v15). " +
                                            $"Instead, use the source name of the variable {nameof(MyConfiguration)}.");
            // New v15.04 - only add if it has not already been set
            // This is to ensure that config masks don't overwrite data which 
            ((DataSourceConfiguration)Configuration).AddIfMissing(key, token);
            if (cacheRelevant)
                CacheRelevantConfigurations.Add(key);
        }
    }
}