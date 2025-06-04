using ToSic.Eav.Context;
using ToSic.Sys.Security.Permissions;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsItems(MultiPermissionsApp.MyServices services)
    : MultiPermissionsApp(services, "Sec.MpItms")
{
    public MultiPermissionsItems Init(IContextOfSite context, IAppIdentity app, IEntity item)
    {
        var l = Log.Fn<MultiPermissionsItems>($"..., appId: {app.AppId}, entityId: {item?.EntityId}");
        Init(context, app);
        _items = [item];
        return l.Return(this);
    }
    private List<IEntity> _items;

    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => _items.ToDictionary(i => i.EntityId.ToString(), BuildItemPermissionChecker);

    /// <summary>
    /// Creates a permission checker for an type in this app
    /// </summary>
    /// <returns></returns>
    private IPermissionCheck BuildItemPermissionChecker(IEntity item)
    {
        var l = Log.Fn<IPermissionCheck>($"type: {item.Type}; entity:{item.EntityId}");
        var checker = BuildPermissionChecker(item.Type, item);
        return l.ReturnAsOk(checker);
    }
}