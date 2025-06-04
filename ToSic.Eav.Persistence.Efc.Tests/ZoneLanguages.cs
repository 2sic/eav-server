using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.Persistence.Efc.Tests;

[Startup(typeof(StartupTestsApps))]
public class ZoneLanguages(EavDbContext db, EfcAppLoader loader) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    private const int MinZones = 4;
    private const int MaxZones = 10;
    private const int AppCountInHomeZone = 7; // ? 6;
    private const int ZoneCountWithMultiLanguage = 2;
        
    private const int ZoneHome = 2;
    private const int ZoneHomeLanguages = 2;
    private const int ZoneHomeLangsActive = 1;

    private const int ZoneMl = 3;
    private const int ZoneMlLanguages = 3;
    private const int ZoneMlLangsActive = 2;

    private const int ZonesWithInactiveLanguages = 2;

#if NETCOREAPP
    [field: AllowNull, MaybeNull]
#endif
    private IDictionary<int, Zone> Zones => field ??= ((IAppsAndZonesLoader)loader.UseExistingDb(db)).Zones();

    private int DefaultAppId => field != 0 ? field : field = Zones.First().Value.DefaultAppId;

    [Fact]
    public void ZonesWithAndWithoutLanguages()
    {
        var mlZones = Zones.Values.Where(z => z.Languages.Count > 1).ToList();
        Equal(ZoneCountWithMultiLanguage, mlZones.Count);//, $"should have {ZoneCountWithMultiLanguage} ml zones");
        var mlWithInactive = mlZones.Where(z => z.Languages.Any(l => !l.Active)).ToList();
        Equal(ZonesWithInactiveLanguages, mlWithInactive.Count);//, $"expect {ZonesWithInactiveLanguages} to have inactive languages");
    }

    [Fact]
    public void HomeZoneLanguages()
    {
        var firstMl = Zones[ZoneHome];
        Equal(ZoneHomeLanguages, firstMl.Languages.Count);//, $"the first zone with ML should have {ZoneHomeLanguages} languages");
        Equal(ZoneHomeLangsActive, firstMl.Languages.Count(l => l.Active));//, $"{ZoneHomeLangsActive} are active");
    }

    [Fact]
    public void ZoneMultiLanguages()
    {
        var firstMl = Zones[ZoneMl];
        Equal(ZoneMlLanguages, firstMl.Languages.Count);//, $"the first zone with ML should have {ZoneHomeLanguages} languages");
        Equal(ZoneMlLangsActive, firstMl.Languages.Count(l => l.Active));//, $"{ZoneHomeLangsActive} are active");
    }

    [Fact]
    public void CountAppsOnMlZone() =>
        Equal(AppCountInHomeZone, Zones[ZoneHome].Apps.Count);//, "app count on second zone");

    [Fact]
    public void HatAtLeastExpertedZoneCount() =>
        InRange(Zones.Count, MinZones, MaxZones);//, $"zone count - often changes, as new test-portals are made. Found: {Zones.Count}");

    [Fact]
    public void DefaultAppIs1() => Equal(1, DefaultAppId);//, "def app on first zone");
}