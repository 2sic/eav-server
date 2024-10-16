using ToSic.Eav.Plumbing;
using ToSic.Eav.Security.Internal;

namespace ToSic.Eav.Context.Internal;

/// <summary>
/// Context of site - fully DI compliant
/// All these objects should normally be injectable
/// In rare cases you may want to replace them, which is why Site/User have Set Accessors
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContextOfSite: ServiceBase<ContextOfSite.MyServices>, IContextOfSite
{
    #region Constructor / DI

    public class MyServices(ISite site, IUser user, Generator<AppPermissionCheck> appPermissionCheck)
        : MyServicesBase(connect: [site, user, appPermissionCheck])
    {
        public ISite Site { get; } = site;
        public IUser User { get; } = user;
        public Generator<AppPermissionCheck> AppPermissionCheck { get; } = appPermissionCheck;
    }
    /// <summary>
    /// Constructor for DI
    /// </summary>
    /// <param name="services"></param>
    public ContextOfSite(MyServices services) : this(services, null) { }

    protected ContextOfSite(MyServices services, string logName) : base(services, logName ?? "Eav.CtxSte")
    {
        Site = Services.Site;
    }
    protected ContextOfSite(MyServicesBase<MyServices> services, string logName) : base(services, logName ?? "Eav.CtxSte")
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
        if (u == null) return false; // note: not sure if user could ever be null since it's injected
        return u.IsSystemAdmin || u.IsSiteAdmin || u.IsSiteDeveloper;
    });

    private bool IsContentAdmin => User?.IsContentAdmin ?? false;
    private bool IsContentEditor => User?.IsContentEditor ?? false;
    private bool ShowDraft => User?.IsEditMode ?? false;

    EffectivePermissions IContextOfUserPermissions.Permissions => _permissions
        ??= UserMayAdmin.Map(mayAdmin => new EffectivePermissions(
            isSiteAdmin: mayAdmin,
            isContentAdmin: mayAdmin || IsContentAdmin,
            isContentEditor: mayAdmin || IsContentEditor,
            showDrafts: ShowDraft));
    private EffectivePermissions _permissions;

    /// <inheritdoc />
    public IContextOfSite Clone(ILog parentLog) => new ContextOfSite(Services, Log.NameId).LinkLog(parentLog);
}