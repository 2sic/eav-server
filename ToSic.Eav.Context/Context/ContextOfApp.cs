using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Data.Sys.PropertyStack;
using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Context;

public record ContextOfApp(
    ISite Site,
    IUser User,
    IAppReader? AppReaderOrNull
): IContextOfApp
{
    /// <summary>
    /// Describe this context without evaluating required or lazy properties, including on derived records.
    /// </summary>
    public sealed override string ToString() => $"{GetType().Name} {{ HasApp = {AppReaderOrNull != null} }}";

    /// <summary>
    /// Helper to lazy-retrieve permissions if ever needed.
    /// </summary>
    public required ContextOfAppFactory InternalMachinery { get; init; }

    /// <summary>
    /// Helper to lazy-retrieve Settings & Resources if ever needed.
    /// </summary>
    public required LazySvc<AppDataStackService> SettingsStackSvc { get; init; }


    public IAppReader AppReaderRequired => AppReaderOrNull ?? throw new NullReferenceException("Trying to get AppReader but AppIdentity is null");

    public IContextOfSite WithSwappedSite(ISite site) => this with { Site = site };


    [field: AllowNull, MaybeNull]
    public EffectivePermissions Permissions
    {
        get => field ??= new(isSiteAdmin: User.MayAdmin(), isContentAdmin: InternalMachinery.GetUserMayEdit(this) || User.IsContentAdmin);
        init;
    }


    [field: AllowNull, MaybeNull]
    public PropertyStack AppSettings => field ??= AppDataStackService.GetStack(AppStackConstants.RootNameSettings);

    [field: AllowNull, MaybeNull]
    public PropertyStack AppResources => field ??= AppDataStackService.GetStack(AppStackConstants.RootNameResources);

    [field: AllowNull, MaybeNull]
    private AppDataStackService AppDataStackService => field ??= SettingsStackSvc.Value.Init(AppReaderRequired);

}
