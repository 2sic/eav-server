using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Generics;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using static System.StringComparison;

namespace ToSic.Eav.DataSource;

partial class DataSourceBase
{
    /// <inheritdoc />
    public IDataSourceConfiguration Configuration => _config.Get(() => Services.Configuration.Attach(this));

    private readonly GetOnce<IDataSourceConfiguration> _config = new();

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
    [PrivateApi]
    protected void ConfigMask(string key, string token, bool cacheRelevant = true)
    {
        const string myConfigOld = "Settings";
        if (token.IndexOf($"[{myConfigOld}:", InvariantCultureIgnoreCase) > -1)
            throw new ArgumentException($"Don't user the source {myConfigOld} for retrieving DS configuration any more (breaking change in v15). " +
                                        $"Instead, use the source name of the variable {nameof(DataSourceConstants.MyConfigurationSourceName)}.");
        // New v15.04 - only add if it has not already been set
        // This is to ensure that config masks don't overwrite data which 
        ((DataSourceConfiguration)Configuration).AddIfMissing(key, token);
        if (cacheRelevant)
            CacheRelevantConfigurations.Add(key);
    }

    [PrivateApi]
    public void Setup(IDataSourceOptions options, IDataSourceLinkable attach)
    {
        var l = Log.Fn();
        var mainUpstream = attach?.Link?.DataSource;
        (this as IAppIdentitySync).UpdateAppIdentity(options?.AppIdentity ?? mainUpstream);
            
        // Attach in-bound, and make it immutable afterwards
        if (attach?.Link == null)
            l.A("Nothing to attach");
        else
            Connect(attach.Link);

        if (options?.Immutable == true) Immutable = true;
        l.A($"{nameof(Immutable)}, {Immutable}");

        var lookUp = options?.LookUp ?? mainUpstream?.Configuration?.LookUpEngine;
        if (lookUp != null && Configuration is DataSourceConfiguration dsConfig)
        {
            l.A("Add lookups");
            dsConfig.LookUpEngine = lookUp;
            var configValues = options?.Values;
            if (configValues != null) dsConfig.AddMany(configValues.ToEditable());
        }
        l.Done();
    }

}