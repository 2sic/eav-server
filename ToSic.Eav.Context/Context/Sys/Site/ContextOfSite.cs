﻿using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Context.Sys.Site;

/// <summary>
/// Context of site - fully DI compliant
/// All these objects should normally be injectable
/// In rare cases you may want to replace them, which is why Site/User have Set Accessors
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContextOfSite: ServiceBase<ContextOfSite.Dependencies>, IContextOfSite
{
    #region Constructor / DI

    public class Dependencies(ISite site, IUser user, Generator<AppPermissionCheck> appPermissionCheck)
        : DependenciesBase(connect: [site, user, appPermissionCheck])
    {
        public ISite Site { get; } = site;
        public IUser User { get; } = user;
        public Generator<AppPermissionCheck> AppPermissionCheck { get; } = appPermissionCheck;
    }

    /// <summary>
    /// Constructor for DI
    /// </summary>
    /// <param name="services"></param>
    public ContextOfSite(Dependencies services) : this(services, null) { }

    protected ContextOfSite(Dependencies services, string? logName, object[]? connect = default) : base(services, logName ?? "Eav.CtxSte", connect: connect)
    {
        Site = Services.Site;
    }

    #endregion


    /// <inheritdoc />
    public ISite Site { get; set; }

    /// <inheritdoc />
    public IUser User => Services.User;

    protected bool UserMayAdmin => Log.Getter(() =>
    {
        var u = User;
        // Note: I'm not sure if the user could ever be null, but maybe in search scenarios?
        if (u == null)
            return false;
        return u.IsSystemAdmin || u.IsSiteAdmin || u.IsSiteDeveloper;
    });

    private bool IsContentAdmin => User?.IsContentAdmin ?? false;
    private bool IsContentEditor => User?.IsContentEditor ?? false;

    [field: AllowNull, MaybeNull]
    EffectivePermissions ICurrentContextUserPermissions.Permissions => field
        ??= UserMayAdmin.Map(mayAdmin => new EffectivePermissions(
            IsSiteAdmin: mayAdmin,
            IsContentAdmin: mayAdmin || IsContentAdmin,
            IsContentEditor: mayAdmin || IsContentEditor,
            ShowDraftData: mayAdmin || IsContentAdmin || IsContentEditor));

    /// <inheritdoc />
    public virtual IContextOfSite Clone(ILog parentLog)
        => new ContextOfSite(Services, Log.NameId).LinkLog(parentLog);
}