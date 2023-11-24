using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Context;
using ToSic.Eav.Persistence.Logging;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.ImportExport;

public class ImportFromRemote: ServiceBase
{
    private readonly IEnvironmentLogger _envLogger;
    private readonly ZipFromUrlImport _zipImportFromUrl;

    #region Constructor / DI

    public ImportFromRemote(IEnvironmentLogger envLogger, ZipFromUrlImport zipImportFromUrl, IUser user) : base("Bck.Export")
    {
        ConnectServices(
            _envLogger = envLogger,
            _zipImportFromUrl = zipImportFromUrl
        );
        _user = user;
    }

    private readonly IUser _user;

    #endregion

    public (bool, List<Message>) InstallPackage(int zoneId, int appId, bool isApp, string packageUrl)
    {
        var l = Log.Fn<(bool, List<Message>)>($"{nameof(zoneId)}:{zoneId}, {nameof(appId)}:{appId}, {nameof(isApp)}:{isApp}, url:{packageUrl}");
            
        l.A("install package:" + packageUrl);
        if (!_user.IsSiteAdmin) throw new Exception("must be admin");
        bool success;

        var importer = _zipImportFromUrl;
        try
        {
            success = importer.Init(zoneId, appId, _user.IsSystemAdmin)
                .ImportUrl(packageUrl, isApp);
        }
        catch (Exception ex)
        {
            _envLogger.LogException(ex);
            throw new Exception("An error occurred while installing the app: " + ex.Message, ex);
        }

        return l.Return((success, importer.Messages), success.ToString());
    }
}