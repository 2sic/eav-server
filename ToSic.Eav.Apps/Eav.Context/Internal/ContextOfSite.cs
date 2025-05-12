using ToSic.Eav.Plumbing;
using ToSic.Eav.Security.Internal;

namespace ToSic.Eav.Context.Internal;

/// <summary>
/// Context of site - fully DI compliant
/// All these objects should normally be injectable
/// In rare cases you may want to replace them, which is why Site/User have Set Accessors
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
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
    protected ContextOfSite(MyServicesBase<MyServices> services, string logName, object[] connect)
        : base(services, logName ?? "Eav.CtxSte", connect: connect)
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
        if (u == null) return false;
        return u.IsSystemAdmin || u.IsSiteAdmin || u.IsSiteDeveloper;
    });

    private bool IsContentAdmin => User?.IsContentAdmin ?? false;
    private bool IsContentEditor => User?.IsContentEditor ?? false;

    EffectivePermissions IContextOfUserPermissions.Permissions => field
        ??= UserMayAdmin.Map(mayAdmin => new EffectivePermissions(
            IsSiteAdmin: mayAdmin,
            IsContentAdmin: mayAdmin || IsContentAdmin,
            IsContentEditor: mayAdmin || IsContentEditor,
            ShowDraftData: mayAdmin || IsContentAdmin || IsContentEditor));

    /// <inheritdoc />
    public IContextOfSite Clone(ILog parentLog) => new ContextOfSite(Services, Log.NameId).LinkLog(parentLog);
}