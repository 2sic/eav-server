using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Context;
using ToSic.Eav.Integration.Security;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;

namespace ToSic.Eav.Security.Internal;

/// <summary>
/// Check permissions on something inside an App, like a specific Entity, Content-Type etc.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppPermissionCheck(IAppReaderFactory appReaders, PermissionCheckBase.MyServices services)
    : PermissionCheckBase(services, $"{AppConstants.LogName}.PrmChk", connect: [appReaders])
{
    #region Constructor & DI

    private readonly EnvironmentPermission _environmentPermission = (EnvironmentPermission)services.EnvironmentPermission;

    public AppPermissionCheck ForItem(IContextOfSite ctx, IAppIdentity appIdentity, IEntity targetItem)
    {
        var l = Log.Fn<AppPermissionCheck>();
        Init(ctx, appIdentity, targetItem?.Type, targetItem, includeApp: true);
        return l.Return(this);
    }

    public AppPermissionCheck ForType(IContextOfSite ctx, IAppIdentity appIdentity, IContentType targetType)
    {
        var l = Log.Fn<AppPermissionCheck>();
        Init(ctx, appIdentity, targetType);
        return l.Return(this);
    }

    public AppPermissionCheck ForAttribute(IContextOfSite ctx, IAppIdentity appIdentity, IContentTypeAttribute attribute)
    {
        var l = Log.Fn<AppPermissionCheck>();
        Init(ctx, appIdentity, permissions: attribute.Permissions);
        return l.Return(this);
    }

    public AppPermissionCheck ForCustom(IContextOfSite ctx, IAppIdentity appIdentity, IEnumerable<IPermission> permissions)
    {
        var l = Log.Fn<AppPermissionCheck>();
        Init(ctx, appIdentity, permissions: permissions);
        return l.Return(this);
    }

    /// <summary>
    /// Init the check for an app.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="appIdentity">The App - in some cases (if no app exists yet) it's null</param>
    /// <returns></returns>
    public AppPermissionCheck ForAppInInstance(IContextOfSite ctx, IAppIdentity appIdentity)
    {
        var l = Log.Fn<AppPermissionCheck>($"ctx, app: {appIdentity?.Show()}");
        Init(ctx, appIdentity, includeApp: true);
        return l.Return(this);
    }

    public AppPermissionCheck ForParts(IContextOfSite ctx, IAppIdentity app, IContentType targetType, IEntity targetItem)
    {
        var l = Log.Fn<AppPermissionCheck>($"ctx, app: {app.Show()}, type: {targetType?.NameId}, item: {targetItem?.EntityId}");
        Init(ctx, app, targetType, targetItem, includeApp: true);
        return l.Return(this);
    }


    /// <summary>
    /// Initialize this object so it can then give information regarding the permissions of an entity.
    /// Uses a GUID as identifier because that survives export/import. 
    /// </summary>
    private void Init(
        IContextOfSite ctx,
        IAppIdentity appIdentity,
        IContentType targetType = null, // optional type to check
        IEntity targetItem = null, // optional entity to check
        IEnumerable<IPermission> permissions = null,
        bool includeApp = false)
    {
        var l = Log.Fn($"..., {targetItem?.EntityId}, app: {appIdentity?.Show()}, {nameof(includeApp)}: {includeApp}");

        // New 2025-03-26 option to include App
        // ATM all cases where App Permissions are included will not have permissions passed in
        // This may change some day
        permissions ??= includeApp
            ? FindPermissionsOfApp(appIdentity)
            : null;

        LoadTargets(targetType ?? targetItem?.Type, targetItem, permissions);
        ((IEnvironmentPermissionSetup)_environmentPermission).Init<IContextOfSite>(ctx, appIdentity);
        Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
        l.Done();
    }


    /// <summary>
    /// Initialize this object so it can then give information regarding the permissions of an entity.
    /// Uses a GUID as identifier because that survives export/import. 
    /// </summary>
    /// <param name="targetType">optional type to check</param>
    /// <param name="targetItem">optional entity to check</param>
    /// <param name="permissions"></param>
    /// 
    private void LoadTargets(IContentType targetType = default, IEntity targetItem = default, IEnumerable<IPermission> permissions = default)
    {
        var l = Log.Fn($"type:{targetType?.NameId}, itm:{targetItem?.EntityGuid} ({targetItem?.EntityId})");

        InitTargets(targetType?.Metadata, targetItem?.Metadata, targetItem?.Owner, permissions);
        l.Done();
    }

    /// <summary>
    /// Retrieve the permissions of an App - in case the App has rules which allow all data to be read by certain users / groups.
    /// </summary>
    /// <param name="appIdentity"></param>
    /// <returns></returns>
    private List<IPermission> FindPermissionsOfApp(IAppIdentity appIdentity)
    {
        var l = Log.Fn<List<IPermission>>();
        var permissions = appIdentity == null
            ? null
            : appReaders.GetOrKeep(appIdentity).Specs.Metadata.Permissions.ToList();
        return l.Return(permissions, $"found: {permissions?.Count}");
    }


    protected IContextOfSite Context { get; private set; }

    #endregion

    #region User Stuff

    /// <summary>
    /// Override base accessor for this
    /// </summary>
    protected override IUser User => Context.User;

    #endregion
}