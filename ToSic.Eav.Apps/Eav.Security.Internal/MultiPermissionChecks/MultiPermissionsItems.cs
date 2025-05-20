using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsItems(MultiPermissionsApp.MyServices services)
    : MultiPermissionsApp(services, "Sec.MpItms")
{
    #region Constructors and DI / Init

    public MultiPermissionsItems Init(IContextOfSite context, IAppIdentity app, IEntity item)
    {
        var l = Log.Fn<MultiPermissionsItems>($"..., appId: {app.AppId}, entityId: {item?.EntityId}");
        Init(context, app);
        _items = [item];
        return l.Return(this);
    }
    private List<IEntity> _items;

    #endregion

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