using System.Collections.ObjectModel;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;

namespace ToSic.Eav.Apps.Tests.Caching;

[CollectionDefinition("AppsCache", DisableParallelization = true)]
public class AppsCacheCollection { }

[Collection("AppsCache")]
public class AppsCacheMultiTenantTests
{
    private IAppsCache CreateAppsCacheTac(IAppCacheKeyService appCacheKeyService)
        => new AppsCacheInMemory(appCacheKeyService);

    [Fact]
    public void ZonesAreIsolatedByAppCacheKey()
    {
        var loaderTenantA = new FakeZoneLoader(() => ZonesWithAppName("TenantA-App"));
        var loaderTenantB = new FakeZoneLoader(() => ZonesWithAppName("TenantB-App"));

        //var toolsTenantA = new FakeAppLoaderTools(loaderTenantA);
        //var toolsTenantB = new FakeAppLoaderTools(loaderTenantB);

        var cacheTenantA = CreateAppsCacheTac(new FixedAppCacheKeyService("tenant-a"));
        var cacheTenantB = CreateAppsCacheTac(new FixedAppCacheKeyService("tenant-b"));

        cacheTenantA.PurgeZones();

        var zonesTenantA = cacheTenantA.Zones(loaderTenantA);
        var zonesTenantB = cacheTenantB.Zones(loaderTenantB);

        Equal("TenantA-App", zonesTenantA[1].Apps[12]);
        Equal("TenantB-App", zonesTenantB[1].Apps[12]);
        Equal(1, loaderTenantA.ZonesCallCount);
        Equal(1, loaderTenantB.ZonesCallCount);
    }

    [Fact]
    public void ZonesDoNotCacheWhenAppCacheKeyMissing()
    {
        var callCount = 0;
        var loader = new FakeZoneLoader(() =>
        {
            var name = $"Call-{Interlocked.Increment(ref callCount)}";
            return ZonesWithAppName(name);
        });

        var cache = CreateAppsCacheTac(new ThrowingAppCacheKeyService());

        cache.PurgeZones();

        var first = cache.Zones(loader);
        var second = cache.Zones(loader);

        NotEqual(first[1].Apps[12], second[1].Apps[12]);
        Equal(2, loader.ZonesCallCount);
    }

    [Fact]
    public void ZonesIncludeGlobalPresetApp()
    {
        var loader = new FakeZoneLoader(() => new Dictionary<int, Zone>());
        //var tools = new FakeAppLoaderTools(loader);
        var cache = CreateAppsCacheTac(new FixedAppCacheKeyService("tenant"));

        cache.PurgeZones();

        var zones = cache.Zones(loader);

        True(zones.ContainsKey(KnownAppsConstants.PresetZoneId));
        var presetZone = zones[KnownAppsConstants.PresetZoneId];
        True(presetZone.Apps.ContainsKey(KnownAppsConstants.PresetAppId));
        True(presetZone.Apps.ContainsKey(KnownAppsConstants.GlobalPresetAppId));
        Equal(KnownAppsConstants.PresetZoneId, cache.ZoneIdOfApp(KnownAppsConstants.GlobalPresetAppId, loader));
    }

    private static IDictionary<int, Zone> ZonesWithAppName(string appName)
    {
        var apps = new Dictionary<int, string>
        {
            { 10, "Default" },
            { 11, "251c0000-eafe-2792-0001-000000000001" },
            { 12, appName }
        };
        var zone = new Zone(1, 10, 11, new ReadOnlyDictionary<int, string>(apps), [
            new()
            {
                Active = true,
                Id = 0,
                EnvironmentKey = "en-us",
                Key = "en-us",
                Name = "English",
                ParentId = null
            }
        ]);
        return new Dictionary<int, Zone> { { 1, zone } };
    }

    private sealed class FixedAppCacheKeyService(string prefix) : IAppCacheKeyService
    {
        public string AppCacheKey(IAppIdentity appIdentity)
            => $"{prefix}:{appIdentity.ZoneId}-{appIdentity.AppId}";
    }

    private sealed class ThrowingAppCacheKeyService : IAppCacheKeyService
    {
        public string AppCacheKey(IAppIdentity appIdentity) => throw new InvalidOperationException("No runtime key");
    }

    //private sealed class FakeAppLoaderTools(IAppsAndZonesLoader loader) : IAppLoaderTools
    //{
    //    public IAppsAndZonesLoader RepositoryLoader(ILog? parentLog) => loader;
    //}

    //private sealed class FakeAppsAndZonesLoader(Func<IDictionary<int, Zone>> zonesFactory) : IAppsAndZonesLoader
    //{
    //    private int _zonesCalls;

    //    public int ZonesCallCount => _zonesCalls;

    //    public ILog? Log => null;

    //    public string PrimaryLanguage { get; set; } = "en-us";

    //    public IAppStateCache AppState(int appId, CodeRefTrail codeRefTrail) =>
    //        throw new NotImplementedException();

    //    public IAppStateCache Update(IAppStateCache app, AppStateLoadSequence startAt, CodeRefTrail codeRefTrail, int[] entityIds) =>
    //        throw new NotImplementedException();

    //    public IDictionary<int, Zone> Zones()
    //    {
    //        _zonesCalls++;
    //        return zonesFactory();
    //    }

    //    public ICollection<IContentType> ContentTypes(int appId, IHasMetadataSourceAndExpiring source) =>
    //        throw new NotImplementedException();
    //}

    private sealed class FakeZoneLoader(Func<IDictionary<int, Zone>> zonesFactory) : IZoneLoader
    {
        private int _zonesCalls;

        public int ZonesCallCount => _zonesCalls;

        public ILog? Log => null;

        public IDictionary<int, Zone> Zones()
        {
            _zonesCalls++;
            return zonesFactory();
        }
    }
}
