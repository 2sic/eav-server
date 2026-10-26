using ToSic.Eav.Context;
using ToSic.Eav.Environment.Sys.Permissions;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// Create permission checkers for various scenarios.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PermissionCheckFactory(IAppReaderFactory appReaders, Generator<PermissionCheck, PermissionCheck.Options> permCheckGen, IEnvironmentPermission envPermission)
    : ServiceBase($"{AppConstants.LogName}.PrmChk", connect: [appReaders, permCheckGen, envPermission])
{
    #region Constructor & DI

    /// <summary>
    /// For something custom, where the permissions list has already been prepared.
    /// </summary>
    public IPermissionCheck For(IContextOfSite ctx, IAppIdentity appIdentity, IEnumerable<IPermission>? permissions, string purpose)
        => Log.Quick(() => Build(ctx, appIdentity, permissions: permissions), message: $"purpose: {purpose}");

    /// <summary>
    /// Init the check for an app (or null in some cases).
    /// </summary>
    /// <param name="ctx">Site context</param>
    /// <param name="appIdentityOrNull">The App - in some cases (if no app exists yet) it's null</param>
    public IPermissionCheck ForApp(IContextOfSite ctx, IAppIdentity? appIdentityOrNull)
        => Log.Quick(() => Build(ctx, appIdentityOrNull, includeApp: true), message: $"ctx, app: {appIdentityOrNull.Show()}");

    /// <summary>
    /// For an item/entity.
    /// </summary>
    public IPermissionCheck ForItem(IContextOfSite ctx, IAppIdentity appIdentity, IEntity targetItem)
        => Log.Quick(() => Build(ctx, appIdentity, targetItem?.Type, targetItem, includeApp: true));

    /// <summary>
    /// For a content-type.
    /// </summary>
    public IPermissionCheck ForType(IContextOfSite ctx, IAppIdentity appIdentity, IContentType targetType)
        => Log.Quick(() =>  Build(ctx, appIdentity, targetType, includeApp: true));


    /// <summary>
    /// Initialize this object so it can then give information regarding the permissions of an entity.
    /// Uses a GUID as identifier because that survives export/import. 
    /// </summary>
    private IPermissionCheck Build(
        IContextOfSite ctxSiteOrBlock,
        IAppIdentity? appIdentity,
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
            ? GetAppPermissions()
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

        return l.Return(permCheckGen.New(options));


        // <summary>
        // Retrieve the permissions of an App - in case the App has rules which allow all data to be read by certain users / groups.
        // </summary>
        List<IPermission>? GetAppPermissions()
        {
            var l2 = Log.Fn<List<IPermission>?>();
            var permList = appIdentity == null
                ? null
                : appReaders.GetOrKeep(appIdentity).Specs.Metadata.Permissions.ToList();
            return l2.Return(permList, $"found: {permList?.Count}");
        }
    }

    #endregion
}