using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.Logging;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Repository.Efc.Tests.ExportXml;

[Startup(typeof(StartupTestFullWithDbAndImport))]
public class ExportImportDataTest(XmlExporter xmlExporter, IAppReaderFactory appReaderFactory) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{

    [Fact(Skip = "ignore for now, reason is that we don't have a mock-IContextResolver")]
    public void ExportData()
    {
        var Log = new Log("TstExp");
        var zoneId = 2;
        var appId = 2;
        var appState = appReaderFactory.Get(new AppIdentity(zoneId, appId));

        var fileXml = xmlExporter
            .Init(new AppExportSpecs(zoneId, appId), appState, false,
                /*contentTypeIdsString?.Split(';') ?? */[],
                /*entityIdsString?.Split(';') ?? */[]
            )
            .GenerateNiceXml();

    }
}