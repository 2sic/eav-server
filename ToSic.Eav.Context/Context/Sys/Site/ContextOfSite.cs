using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Context.Sys.Site;

/// <summary>
/// Context of site - fully DI compliant
/// All these objects should normally be injectable
/// In rare cases you may want to replace them, which is why Site/User have Set Accessors
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContextOfSite(ContextOfSite.Dependencies services)
    : ServiceBase<ContextOfSite.Dependencies>(services, "Eav.CtxSte"), IContextOfSite
{
    /// <summary>
    /// Dependencies
    /// </summary>
    public record Dependencies(ISite Site, IUser User)
        : DependenciesBase(connect: [Site, User]);


    /// <inheritdoc />
    public ISite Site => Services.Site;

    /// <inheritdoc />
    public IUser User => field ??= Services.User;

    public virtual IContextOfSite WithSwappedSite(ISite site) => new ContextOfSite(new(site, User));

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    EffectivePermissions ICurrentContextUserPermissions.Permissions => field
        ??= EffectiveAppPermissions();

    private EffectivePermissions EffectiveAppPermissions()
    {
        var mayAdmin = GetUserMayAdmin(User);
        var isContentAdmin = User?.IsContentAdmin ?? false;
        var isContentEditor = User?.IsContentEditor ?? false;
        return new(
            IsSiteAdmin: mayAdmin,
            IsContentAdmin: mayAdmin || isContentAdmin,
            IsContentEditor: mayAdmin || isContentEditor,
            ShowDraftData: mayAdmin || isContentAdmin || isContentEditor);
    }

    internal static bool GetUserMayAdmin(IUser user) => user != null! // Note: I'm not sure if the user could ever be null, but maybe in search scenarios?
                                                    && (user.IsSystemAdmin || user.IsSiteAdmin || user.IsSiteDeveloper);


    ///// <inheritdoc />
    //IContextOfSite IContextOfSiteOrMoreCloneable.CloneFullContext(ISite site)
    //    => new ContextOfSite(Services with { Site = site });
}