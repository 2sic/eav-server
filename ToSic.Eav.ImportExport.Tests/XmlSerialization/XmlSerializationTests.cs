using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Repositories;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

namespace ToSic.Eav.ImportExport.Tests.XmlSerialization;

[Startup(typeof(StartupTestsApps))]
public class XmlSerializationTests(XmlSerializer xmlSerializer, IAppsAndZonesLoaderWithRaw repoLoader, IAppsCatalog appsCatalog, ITestOutputHelper output) : IClassFixture<DoFixtureStartup<ScenarioMini>>
{

    [Fact]
    public void Xml_SerializeItemOnHome()
    {
        var test = new SpecsTestExportSerialize();
        var appReader = repoLoader.AppStateReaderRawTac(test.AppId);
        //var zone = new ZoneRuntime().Init(test.ZoneId, Log);
        var languageMap = appsCatalog
            .Zone(test.ZoneId).LanguagesActive
            .ToDictionary(l => l.EnvironmentKey.ToLowerInvariant(), l => l.DimensionId);
        var exBuilder = xmlSerializer.Init(languageMap, appReader);
        var xmlEnt = exBuilder.Serialize(test.TestItemToSerialize);
        True(xmlEnt.Length > 200, "should get a long xml string");
        output.WriteLine(xmlEnt);
        //Assert.Equal(xmlstring, xmlEnt, "xml strings should be identical");
    }

    [Fact]
    public void Xml_CompareAllSerializedEntitiesOfApp()
    {
        var test = new SpecsTestExportSerialize();
        var appId = test.AppId;
        var appReader = repoLoader.AppStateReaderRawTac(appId);
        var languageMap = appsCatalog
            .Zone(test.ZoneId).LanguagesActive
            .ToDictionary(l => l.EnvironmentKey.ToLowerInvariant(), l => l.DimensionId);
        var exBuilder = xmlSerializer.Init(languageMap, appReader);

        var maxCount = 500;
        var skip = 0;
        var count = 0;
        try
        {
            foreach (var appEntity in appReader.List)
            {
                // maybe skip some
                if (count++ < skip) continue;

                //var xml = xmlbuilder.XmlEntity(appEntity.EntityId);
                //var xmlstring = xml.ToString();
                var xmlEnt = exBuilder.Serialize(appEntity.EntityId);
                //Assert.Equal(xmlstring, xmlEnt,
                //    $"xml of item {count} strings should be identical - but was not on xmlEnt {appEntity.EntityId}");

                // stop if we ran enough tests
                if (count >= maxCount)
                    return;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"had issue after count{count}", ex);
        }


    }
}