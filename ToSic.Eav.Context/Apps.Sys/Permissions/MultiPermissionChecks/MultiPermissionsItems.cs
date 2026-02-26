using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsItems(MultiPermissionsApp.Dependencies services)
    : MultiPermissionsApp(services, "Sec.MpItms"),
        IServiceWithSetup<MultiPermissionsItems.Options>,
        IHasOptions<MultiPermissionsItems.Options>
{
    public new record Options: MultiPermissionsApp.Options
    {
        public List<IEntity> Entities { get; init; } = [];
    }

    public new Options MyOptions => (Options)base.MyOptions;


    public void Setup(Options options) => base.Setup(options);


    //public MultiPermissionsItems Init(IContextOfSite context, IAppIdentity app, IEntity item)
    //{
    //    var l = Log.Fn<MultiPermissionsItems>($"..., appId: {app.AppId}, entityId: {item.EntityId}");
    //    Init(context, app);
    //    _items = [item];
    //    return l.Return(this);
    //}

    //private List<IEntity> _items = [];

    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => MyOptions.Entities.ToDictionary(i => i.EntityId.ToString(), BuildItemPermissionChecker);

    /// <summary>
    /// Creates a permission checker for an entity in this app
    /// </summary>
    /// <returns></returns>
    private IPermissionCheck BuildItemPermissionChecker(IEntity item)
    {
        var l = Log.Fn<IPermissionCheck>($"type: {item.Type}; entity:{item.EntityId}");
        var checker = BuildPermissionChecker(item.Type, item);
        return l.ReturnAsOk(checker);
    }
}