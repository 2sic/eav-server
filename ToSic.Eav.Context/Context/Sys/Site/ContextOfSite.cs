using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

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

    public record Dependencies(ISite Site, IUser User)
        : DependenciesBase(connect: [Site, User]);

    /// <summary>
    /// Constructor for DI
    /// </summary>
    /// <param name="services"></param>
    public ContextOfSite(Dependencies services) : this(services, null) { }

    protected ContextOfSite(Dependencies services, string? logName, object[]? connect = default) : base(services, logName ?? "Eav.CtxSte", connect: connect)
    {
    }

    #endregion


    /// <inheritdoc />
    public ISite Site => Services.Site;

    /// <inheritdoc />
    public IUser User => field ??= Services.User;

    protected bool UserMayAdmin => Log.Getter(() => User != null! // Note: I'm not sure if the user could ever be null, but maybe in search scenarios?
                                                    && (User.IsSystemAdmin || User.IsSiteAdmin || User.IsSiteDeveloper));

    [field: AllowNull, MaybeNull]
    EffectivePermissions ICurrentContextUserPermissions.Permissions => field
        ??= EffectiveAppPermissions();

    private EffectivePermissions EffectiveAppPermissions()
    {
        var mayAdmin = UserMayAdmin;
        var isContentAdmin = User?.IsContentAdmin ?? false;
        var isContentEditor = User?.IsContentEditor ?? false;
        return new(
            IsSiteAdmin: mayAdmin,
            IsContentAdmin: mayAdmin || isContentAdmin,
            IsContentEditor: mayAdmin || isContentEditor,
            ShowDraftData: mayAdmin || isContentAdmin || isContentEditor);
    }

    /// <inheritdoc />
    public virtual IContextOfSite CloneFullContext(ILog parentLog, ISite site)
        => new ContextOfSite(Services with { Site = site }, Log.NameId).LinkLog(parentLog);
}