using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Eav.ImportExport.Sys.XmlList;

namespace ToSic.Eav.ImportExport.Sys.XmlData;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportDataXml(LazySvc<ImportListXml> importListXml, IAppCachePurge appCachePurge, IAppsCatalog appsCatalog, DataImportLogSettings importLogSettings)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtIm", connect: [importListXml, appCachePurge, appsCatalog, importLogSettings])
{
    
    public ContentImportResult XmlImport(ContentFileImportOptions args)
    {
        var l = Log.Fn<ContentImportResult>(args.DebugInfo);

        var import = PrepareXmlImport(args);

        if (import.ErrorLog.HasErrors)
            return l.Return(new(false, import.ErrorLog.Errors), "done, errors: true");

        import.PersistImportToRepository();
        appCachePurge.Purge(appsCatalog.AppIdentity(args.AppId));

        return l.Return(new(!import.ErrorLog.HasErrors, import.ErrorLog.Errors), $"done, errors: {import.ErrorLog.HasErrors}");
    }

    public ImportListXml PrepareXmlImport(ContentFileImportOptions args)
    {
        var l = Log.Fn<ImportListXml>($"prepare xml import {args.DebugInfo}");
        var contextLanguages = appsCatalog
            .Zone(MyOptions.ZoneId)
            .LanguagesActive
            .Select(lng => lng.EnvironmentKey)
            .ToArray();

        using var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64));
        var importer = importListXml.Value.Init(
            MyOptions.AppReader,
            args.ContentType,
            contentSteam,
            contextLanguages,
            args.DefaultLanguage,
            args.ClearEntities,
            args.ImportResourcesReferences,
            importLogSettings.GetLogSettings()
        );
        return l.Return(importer);
    }
}
