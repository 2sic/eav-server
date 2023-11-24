using ToSic.Eav.Apps.Security;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context;

/// <summary>
/// Context of site - fully DI compliant
/// All these objects should normally be injectable
/// In rare cases you may want to replace them, which is why Site/User have Set Accessors
/// </summary>
public class ContextOfSite: ServiceBase<ContextOfSite.MyServices>, IContextOfSite
{
    #region Constructor / DI

    public class MyServices: MyServicesBase
    {
        public ISite Site { get; }
        public IUser User { get; }
        public Generator<AppPermissionCheck> AppPermissionCheck { get; }

        public MyServices(
            ISite site,
            IUser user,
            Generator<AppPermissionCheck> appPermissionCheck
        )
        {
            ConnectServices(
                Site = site,
                User = user,
                AppPermissionCheck = appPermissionCheck
            );
        }
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

    /// <inheritdoc />
    public virtual bool UserMayEdit => Log.Getter(() =>
    {
        var u = User;
        if (u == null) return false;
        return u.IsSystemAdmin || u.IsSiteAdmin || u.IsSiteDeveloper;
    });

    /// <inheritdoc />
    public IContextOfSite Clone(ILog parentLog) => new ContextOfSite(Services, Log.NameId).LinkLog(parentLog);
}