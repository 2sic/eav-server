using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.XmlExport;
using ToSic.Eav.Testing.Scenarios;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Repository.Efc.Tests.ExportXml;

[Startup(typeof(StartupTestsApps))]
public class ExportImportDataTest(XmlExporter xmlExporter, IAppReaderFactory appReaderFactory) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{

    [Fact(Skip = "ignore for now, reason is that we don't have a mock-IContextResolver")]
    public void ExportData()
    {
        var Log = new Log("TstExp");
        var zoneId = 2;
        var appId = 2;
        var specs = new AppExportSpecs(zoneId, appId);
        var appReader = appReaderFactory.GetTac(specs);

        var fileXml = xmlExporter
            .Init(specs, appReader, false,
                /*contentTypeIdsString?.Split(';') ?? */[],
                /*entityIdsString?.Split(';') ?? */[]
            )
            .GenerateNiceXml();

    }
}