//using ToSic.Sys.Users;
//using ToSic.Sys.Users.Permissions;

//namespace ToSic.Eav.Context.Sys.Site;

///// <summary>
///// Context of site - fully DI compliant
///// All these objects should normally be injectable
///// In rare cases you may want to replace them, which is why Site/User have Set Accessors
///// </summary>
//[ShowApiWhenReleased(ShowApiMode.Never)]
//public class ContextOfSite(ISite site, IUser user)
//    : ServiceBase("Eav.CtxSte"), IContextOfSite
//{
//    /// <inheritdoc />
//    public ISite Site => site;

//    /// <inheritdoc />
//    public IUser User => user;

//    public virtual IContextOfSiteNew WithSwappedSite(ISite site)
//        => new ContextOfSite(site, User);

//    /// <inheritdoc />
//    [field: AllowNull, MaybeNull]
//    public EffectivePermissions Permissions
//        => field ??= User.EffectivePermissions();
//}