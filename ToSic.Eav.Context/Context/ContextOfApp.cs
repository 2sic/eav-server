using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Context.Sys.Site;
using ToSic.Eav.Data.Sys.PropertyStack;
using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Context;

public record ContextOfApp(
    ISite Site,
    IUser User,
    IAppReader? AppReaderOrNull): IContextOfApp
{
    public required ContextOfAppFactory InternalMachinery { get; init; }
    public required LazySvc<AppDataStackService> SettingsStackSvc { get; init; }


    public IAppReader AppReaderRequired => AppReaderOrNull ?? throw new NullReferenceException("Trying to get AppReader but AppIdentity is null");

    public IContextOfSite WithSwappedSite(ISite site) => this with { Site = site };



    internal bool UserMayAdmin => _userMayAdmin ??= ContextOfSite.GetUserMayAdmin(User);
    private bool? _userMayAdmin;


    [field: AllowNull, MaybeNull]
    EffectivePermissions ICurrentContextUserPermissions.Permissions
        => field ??= new(isSiteAdmin: UserMayAdmin, isContentAdmin: InternalMachinery.GetUserMayEdit(this) || User.IsContentAdmin);




    [field: AllowNull, MaybeNull]
    public PropertyStack AppSettings => field ??= AppDataStackService.GetStack(AppStackConstants.RootNameSettings);

    [field: AllowNull, MaybeNull]
    public PropertyStack AppResources => field ??= AppDataStackService.GetStack(AppStackConstants.RootNameResources);

    [field: AllowNull, MaybeNull]
    private AppDataStackService AppDataStackService => field ??= SettingsStackSvc.Value.Init(AppReaderRequired);

}