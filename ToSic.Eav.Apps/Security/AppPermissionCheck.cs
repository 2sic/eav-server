using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security;

/// <summary>
/// Check permissions on something inside an App, like a specific Entity, Content-Type etc.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppPermissionCheck: PermissionCheckBase
{
    #region Constructor & DI
    public AppPermissionCheck(IAppStates appStates, MyServices services) : base(services, $"{AppConstants.LogName}.PrmChk")
    {
        ConnectServices(
            _appStates = appStates,
            _environmentPermission = (EnvironmentPermission)services.EnvironmentPermission
        );
    }
    private readonly IAppStates _appStates;
    private readonly EnvironmentPermission _environmentPermission;

    public AppPermissionCheck ForItem(IContextOfSite ctx, IAppIdentity appIdentity, IEntity targetItem) => Log.Func(() =>
    {
        Init(ctx, appIdentity, targetItem?.Type, targetItem);
        return this;
    });

    public AppPermissionCheck ForType(IContextOfSite ctx, IAppIdentity appIdentity, IContentType targetType) => Log.Func(() =>
    {
        Init(ctx, appIdentity, targetType);
        return this;
    });

    public AppPermissionCheck ForAttribute(IContextOfSite ctx, IAppIdentity appIdentity, IContentTypeAttribute attribute) => Log.Func(() =>
    {
        Init(ctx, appIdentity, permissions: attribute.Permissions);
        return this;
    });

    public AppPermissionCheck ForCustom(IContextOfSite ctx, IAppIdentity appIdentity, IEnumerable<Permission> permissions) => Log.Func(() =>
    {
        Init(ctx, appIdentity, permissions: permissions);
        return this;
    });

    /// <summary>
    /// Init the check for an app.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="appIdentity">The App - in some cases (if no app exists yet) it's null</param>
    /// <returns></returns>
    public AppPermissionCheck ForAppInInstance(IContextOfSite ctx, IAppIdentity appIdentity)
    {
        var l = Log.Fn<AppPermissionCheck>($"ctx, app: {appIdentity}");
        var permissions = FindPermissionsOfApp(appIdentity);
        Init(ctx, appIdentity, permissions: permissions);
        return l.Return(this, $"Permissions: {permissions?.Count}");
    }

    private List<Permission> FindPermissionsOfApp(IAppIdentity appIdentity)
    {
        
        var permissions = appIdentity == null
            ? null
            : (appIdentity as IApp)?.Metadata.Permissions.ToList()
              ?? (appIdentity as AppState)?.Metadata.Permissions.ToList()
              ?? (appIdentity as IAppState)?.Metadata.Permissions?.ToList()
              ??  _appStates.Get(appIdentity).Metadata.Permissions.ToList();
        return permissions;
    }

    public AppPermissionCheck ForParts(IContextOfSite ctx, IAppIdentity app, IContentType targetType, IEntity targetItem) => Log.Func(() =>
    {
        Init(ctx, app, targetType, targetItem, FindPermissionsOfApp(app));
        return this;
    });


    /// <summary>
    /// Initialize this object so it can then give information regarding the permissions of an entity.
    /// Uses a GUID as identifier because that survives export/import. 
    /// </summary>
    private void Init(
        IContextOfSite ctx,
        IAppIdentity appIdentity,
        IContentType targetType = null, // optional type to check
        IEntity targetItem = null, // optional entity to check
        IEnumerable<Permission> permissions = null)
    {
        Init(targetType ?? targetItem?.Type, targetItem, permissions);
        _environmentPermission.Init(ctx, appIdentity);
        Log.Do($"..., {targetItem?.EntityId}, app: {appIdentity?.AppId}, ", () =>
        {
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
            AppIdentity = appIdentity;
        });
    }

    protected IContextOfSite Context { get; private set; }


    protected IAppIdentity AppIdentity;


    #endregion

    #region User Stuff

    /// <summary>
    /// Override base accessor for this
    /// </summary>
    protected override IUser User => Context.User;

    #endregion
}