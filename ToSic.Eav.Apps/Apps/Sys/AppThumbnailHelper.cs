using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Apps.Sys;

public static class AppThumbnailHelper
{
    public static string? GetUrl(IAppReader appReader, IAppPaths appPaths, LazySvc<GlobalPaths> globalPaths)
    {
        // Primary app - we only PiggyBack cache the icon in this case
        // Because otherwise the icon could get moved, and people would have a hard time seeing the effect
        if (appReader.Specs.IsSiteSettingsApp())
            return appReader.GetCache().PiggyBackGet("app-thumbnail-primary",
                () => globalPaths.Value.GlobalPathTo(AppConstants.AppPrimaryIconFile, PathTypes.Link));

        // standard app (not global) try to find app-icon in its (portal) app folder
        if (!appReader.IsShared() && File.Exists($"{appPaths.PhysicalPath}/{AppConstants.AppIconFile}"))
            return $"{appPaths.Path}/{AppConstants.AppIconFile}";

        // global app (and standard app without app-icon in its portal folder) looks for app-icon in global shared location 
        if (File.Exists($"{appPaths.PhysicalPathShared}/{AppConstants.AppIconFile}"))
            return $"{appPaths.PathShared}/{AppConstants.AppIconFile}";

        return null;
    }

}