using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.ImportExport.Sys.XmlList;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Tests.AppXml;

public class XmlTable(IAppsAndZonesLoaderWithRaw loader, ExportListXml exportListXml)
{

    public new static ILog Log = new Log("TstXml");
    private int AppId = 78;

    [Fact(Skip = "Not done")]
    
    public void XmlTable_ResolveWithFullFallback()
    {
        var exporter = BuildExporter(AppId, "BlogPost");

        // todo: need ML portal for testing

    }

    private ExportListXml BuildExporter(int appId, string ctName)
    {
        //var loader = GetService<IRepositoryLoader>();
        var appPackage = loader.AppStateReaderRawTac(appId);
        var type = appPackage.ContentTypes.First(ct => ct.Name == ctName);
        return exportListXml.Init(appPackage, type);
    }
}