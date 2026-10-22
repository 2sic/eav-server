using ToSic.Eav.Context;
using ToSic.Eav.Environment.Sys.Permissions;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// Create permission checkers for various scenarios.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PermissionCheckFactory(IAppReaderFactory appReaders, Generator<PermissionCheck, PermissionCheck.Options> permissionCheck, IEnvironmentPermission envPermission)
    : ServiceBase($"{AppConstants.LogName}.PrmChk", connect: [appReaders, permissionCheck, envPermission])
{
    #region Constructor & DI

    public IPermissionCheck ForItem(IContextOfSite ctx, IAppIdentity appIdentity, IEntity targetItem)
    {
        var l = Log.Fn<IPermissionCheck>();
        var result = Init(ctx, appIdentity, targetItem?.Type, targetItem, includeApp: true);
        return l.Return(result);
    }

    public IPermissionCheck ForType(IContextOfSite ctx, IAppIdentity appIdentity, IContentType targetType)
    {
        var l = Log.Fn<IPermissionCheck>();
        var result = Init(ctx, appIdentity, targetType);
        return l.Return(result);
    }

    public IPermissionCheck For(string purpose, IContextOfSite ctx, IAppIdentity appIdentity, IEnumerable<IPermission>? permissions)
    {
        var l = Log.Fn<IPermissionCheck>($"For: {purpose}");
        var result = Init(ctx, appIdentity, permissions: permissions);
        return l.Return(result);
    }

    /// <summary>
    /// Init the check for an app.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="appIdentity">The App - in some cases (if no app exists yet) it's null</param>
    /// <returns></returns>
    public IPermissionCheck ForAppInInstance(IContextOfSite ctx, IAppIdentity appIdentity)
    {
        var l = Log.Fn<IPermissionCheck>($"ctx, app: {appIdentity.Show()}");
        var result = Init(ctx, appIdentity, includeApp: true);
        return l.Return(result);
    }

    public IPermissionCheck ForParts(IContextOfSite ctx, IAppIdentity app, IContentType? targetType, IEntity? targetItem)
    {
        var l = Log.Fn<IPermissionCheck>($"ctx, app: {app.Show()}, type: {targetType?.NameId}, item: {targetItem?.EntityId}");
        var result = Init(ctx, app, targetType, targetItem, includeApp: true);
        return l.Return(result);
    }


    /// <summary>
    /// Initialize this object so it can then give information regarding the permissions of an entity.
    /// Uses a GUID as identifier because that survives export/import. 
    /// </summary>
    private IPermissionCheck Init(
        IContextOfSite ctxSiteOrBlock,
        IAppIdentity appIdentity,
        IContentType? targetType = null, // optional type to check
        IEntity? targetItem = null, // optional entity to check
        IEnumerable<IPermission>? permissions = null,
        bool includeApp = false)
    {
        var l = Log.Fn<IPermissionCheck>($"..., {targetItem?.EntityId}, app: {appIdentity.Show()}, {nameof(includeApp)}: {includeApp}");

        // New 2025-03-26 option to include App
        // ATM all cases where App Permissions are included will not have permissions passed in
        // This may change some day
        permissions ??= includeApp
            ? FindPermissionsOfApp(appIdentity)
            : null;

        var env = ((IEnvironmentPermissionSetup)envPermission).Init<IContextOfSite>(ctxSiteOrBlock, appIdentity);
        var user = (ctxSiteOrBlock ?? throw new ArgumentNullException(nameof(ctxSiteOrBlock))).User;
        var finalTypeOrNull = targetType ?? targetItem?.Type;
        l.A($"type:{finalTypeOrNull?.NameId}, itm:{targetItem?.EntityGuid} ({targetItem?.EntityId})");
        var options = new PermissionCheck.Options(
            PermissionCheck.CombinePermissions([finalTypeOrNull?.Metadata, targetItem?.Metadata], permissions),
            targetItem?.Owner,
            env,
            user);

        return l.Return(permissionCheck.New(options));
    }


    /// <summary>
    /// Retrieve the permissions of an App - in case the App has rules which allow all data to be read by certain users / groups.
    /// </summary>
    /// <param name="appIdentity"></param>
    /// <returns></returns>
    private List<IPermission>? FindPermissionsOfApp(IAppIdentity? appIdentity)
    {
        var l = Log.Fn<List<IPermission>?>();
        var permissions = appIdentity == null
            ? null
            : appReaders.GetOrKeep(appIdentity).Specs.Metadata.Permissions.ToList();
        return l.Return(permissions, $"found: {permissions?.Count}");
    }


    #endregion
}