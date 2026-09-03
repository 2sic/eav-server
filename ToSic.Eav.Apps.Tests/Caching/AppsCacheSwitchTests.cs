using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Tests.Caching;

public class AppsCacheSwitchTests
{
    private const string DefaultKey = "DefaultCache";

    [Theory]
    [InlineData(false, false, true, true, typeof(DefaultCache))]
    [InlineData(true, false, true, true, typeof(WebFarmCache))]
    [InlineData(true, true, true, true, typeof(WebFarmDebugCache))]
    [InlineData(true, true, true, false, typeof(WebFarmCache))]
    [InlineData(true, false, false, true, typeof(DefaultCache))]
    public void Value_SelectsBestAvailableEnabledCache(
        bool enableWebFarm,
        bool enableDebug,
        bool registerWebFarm,
        bool registerDebug,
        Type expectedType)
    {
        var services = new ServiceCollection()
            .AddKeyedSingleton<IAppsCache, DefaultCache>(DefaultKey);
        if (registerWebFarm)
            services.AddKeyedSingleton<IAppsCache, WebFarmCache>(nameof(BuiltInFeatures.WebFarmCache));
        if (registerDebug)
            services.AddKeyedSingleton<IAppsCache, WebFarmDebugCache>(nameof(BuiltInFeatures.WebFarmCacheDebug));
        using var provider = services.BuildServiceProvider();
        var features = new TestFeaturesService(enableWebFarm, enableDebug);
        var sut = new AppsCacheSwitch(provider, features, null!);

        var result = sut.Value;

        IsType(expectedType, result);
    }

    private sealed class TestFeaturesService(bool enableWebFarm, bool enableDebug) : ISysFeaturesService
    {
        private readonly HashSet<string> _enabled = new(
            [
                enableWebFarm ? nameof(BuiltInFeatures.WebFarmCache) : string.Empty,
                enableDebug ? nameof(BuiltInFeatures.WebFarmCacheDebug) : string.Empty
            ],
            StringComparer.Ordinal);

        public bool IsEnabled(params Feature[] features) => features.All(feature => _enabled.Contains(feature.NameId));
        public bool IsEnabled(params string[] nameIds) => nameIds.All(_enabled.Contains);
        public bool IsEnabled(Guid guid) => false;
        public bool IsEnabled(IEnumerable<Guid> guids) => false;
        public bool IsEnabled(IEnumerable<Guid> features, string message, [NotNullWhen(false)] out FeaturesDisabledException? exception)
        {
            exception = new(message);
            return false;
        }
        public string MsgMissingSome(params Guid[] ids) => string.Empty;
        public FeatureState? Get(string nameId) => null;
        public IEnumerable<FeatureState> All => [];
        public IEnumerable<FeatureState> UiFeaturesForEditors => [];
        public bool Valid => true;
        public FeatureStatesPersisted? Stored => null;
        public bool UpdateFeatureList(FeatureStatesPersisted newList, IList<FeatureState> sysFeatures) => false;
        public long CacheTimestamp => 0;
        public bool CacheChanged(long dependentTimeStamp) => false;
        public bool CacheIsNotifyOnly => true;
        public string CacheDependencyId => nameof(TestFeaturesService);
    }

    private abstract class TestAppsCache : IAppsCache
    {
        public IAppStateCache Get(IAppIdentity app, IAppLoaderTools tools) => null!;
        public IReadOnlyDictionary<int, Zone> Zones(IAppLoaderTools tools) => new Dictionary<int, Zone>();
        public int ZoneIdOfApp(int appId, IAppLoaderTools tools) => 0;
        public bool Has(IAppIdentity app) => false;
        public void Purge(IAppIdentity app) { }
        public void PurgeZones() { }
        public void Update(IAppIdentity app, IEnumerable<int> entities, ILog log, IAppLoaderTools tools) { }
        public void Add(IAppStateCache appState) { }
        public void Load(IAppIdentity app, string primaryLanguage, IAppLoaderTools tools) { }
    }

    private sealed class DefaultCache : TestAppsCache;
    private sealed class WebFarmCache : TestAppsCache;
    private sealed class WebFarmDebugCache : TestAppsCache;
}
