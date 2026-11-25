using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.ImportExport.Integration;

#pragma warning disable CS9113
public class ImportExportPathsAndOptions(ISite site, IAppReaderFactory appReaders, IAppsCatalog appsCatalog, IAppPathsMicroSvc appPathsSvc)
    : ServiceBase("Dnn.ImExPth", connect: [site, appReaders, appPathsSvc])
{
    public string TargetPath(string folder) => Path.Combine(site.AppsRootPhysicalFull, folder);

    public string FallbackContentTypeScope => ScopeConstants.Default;

    public string DefaultLanguage => site.DefaultCultureCode;

    public string TemplatesRoot(int zoneId, int appId)
        => AppPaths(zoneId, appId).PhysicalPath;

    public string GlobalTemplatesRoot(int zoneId, int appId)
        => AppPaths(zoneId, appId).PhysicalPathShared;

    private IAppPaths AppPaths(int zoneId, int appId) => _appPaths
        ??= appPathsSvc.Get(appReaders.Get(new AppIdentity(zoneId, appId)), site);
    private IAppPaths? _appPaths;

    public SaveOptions SaveOptions(int zoneId)
    {
        var l = Log.Fn<SaveOptions>($"{nameof(zoneId)}:{zoneId}");

        var langs = appsCatalog.Zone(zoneId).Languages;
        var opts = new SaveOptions
        {
            PrimaryLanguage = DefaultLanguage,
            Languages = langs,
        };

        return l.Return(opts, $"langs: {langs.Count}");
    }
}
