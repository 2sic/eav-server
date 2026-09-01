using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class AppFinder(IAppsCatalog appsCatalog, IAppReaderFactory appReaders) : ServiceBase("App.ZoneRt")
{
    /// <summary>
    /// Find the app id from the app-name (usually a guid or "Default").
    /// Can also check the folder name
    /// </summary>
    public int FindAppId(int zoneId, string? appName, bool alsoCheckFolderName = false) 
    {
        var l = Log.Fn<int>($"{nameof(zoneId)}:{zoneId}, {nameof(appName)}:{appName}, {nameof(alsoCheckFolderName)}:{alsoCheckFolderName}");
        try
        {
            if (appName.IsEmptyOrWs())
                return l.Return(KnownAppsConstants.AppIdEmpty, "no name");

            var nameLower = appName.ToLowerInvariant();
            var appId = appsCatalog.Apps(zoneId)
                .Where(p => p.Value.EqualsInsensitive(nameLower))
                .Select(p => p.Key).FirstOrDefault();

            // optionally check folder names
            if (appId == 0 && alsoCheckFolderName)
                appId = AppIdFromFolderName(zoneId, appName);

            var final = appId > 0 ? appId : AppConstants.AppIdNotFound;
            return l.ReturnAndLog(final);
        }
        catch (Exception ex)
        {
            l.Done(ex);
            throw;
        }
    }

    /// <summary>
    /// Find an app based on the folder name. Will check the App Metadata for this
    /// </summary>
    public int AppIdFromFolderName(int zoneId, string folderName)
    {
        var l = Log.Fn<int>($"{nameof(zoneId)}: {zoneId}; {nameof(folderName)}: {folderName}");
        try
        {
            foreach (var p in appsCatalog.Apps(zoneId))
            {
                // Maybe TryGet, but since we're going through the zone-apps, they must all exist
                var appReader = appReaders.Get(new AppIdentity(zoneId, p.Key));
                var appSpecs = appReader.Specs;
                if (appSpecs.Folder.EqualsInsensitive(folderName))
                    return l.Return(p.Key, "folder matched");
            }

            // not found
            return l.Return(AppConstants.AppIdNotFound, "not found");
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            l.Done();
            throw;
        }
    }
}