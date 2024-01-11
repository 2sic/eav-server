using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MultiPermissionsItems(MultiPermissionsApp.MyServices services)
    : MultiPermissionsApp(services, "Sec.MpItms")
{
    #region Constructors and DI / Init

    public MultiPermissionsItems Init(IContextOfSite context, IAppIdentity app, IEntity item)
    {
        Init(context, app);
        _items = new List<IEntity> {item};
        return this;
    }
    private List<IEntity> _items;

    #endregion

    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => _items.ToDictionary(i => i.EntityId.ToString(), BuildItemPermissionChecker);

    /// <summary>
    /// Creates a permission checker for an type in this app
    /// </summary>
    /// <returns></returns>
    private IPermissionCheck BuildItemPermissionChecker(IEntity item) =>
        Log.Func($"{item.EntityId}", () => (BuildPermissionChecker(item.Type, item), "ok"));
}