using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class AppFinder(IAppStates appStates, IAppReaders appReaders) : ServiceBase("App.ZoneRt")
{
    /// <summary>
    /// Find the app id from the app-name (usually a guid or "Default").
    /// Can also check the folder name
    /// </summary>
    public int FindAppId(int zoneId, string appName, bool alsoCheckFolderName = false) 
    {
        var l = Log.Fn<int>($"{nameof(zoneId)}:{zoneId}, {nameof(appName)}:{appName}, {nameof(alsoCheckFolderName)}:{alsoCheckFolderName}");
        try
        {
            if (appName.IsEmptyOrWs())
                return l.Return(Constants.AppIdEmpty, "no name");

            var nameLower = appName.ToLowerInvariant();
            var appId = appStates.AppsCatalog.Apps(zoneId)
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
            foreach (var p in appStates.AppsCatalog.Apps(zoneId))
            {
                var appState = appReaders.GetReader(new AppIdentity(zoneId, p.Key));
                if (appState.Folder.EqualsInsensitive(folderName))
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

    /// <summary>
    /// Find an app based on the app name. Will check the App Metadata for this
    /// </summary>
    public int AppIdFromAppName(int zoneId, string appName) => Log.Func(appName, () =>
    {
        var nameLower = appName.ToLowerInvariant();

        foreach (var p in appStates.AppsCatalog.Apps(zoneId))
        {
            var appState = appReaders.GetReader(new AppIdentity(zoneId, p.Key));

            if (!string.IsNullOrEmpty(appState.Name) && appState.Name.ToLowerInvariant() == nameLower)
                return (p.Key, "name matched");
        }

        // not found
        return (AppConstants.AppIdNotFound, "not found");
    });
}