using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsItems(PermissionContextAdjuster permCtxAdjuster, PermissionCheckFactory permCheckFactory)
    : ServiceWithSetup<MultiPermissionsItems.Options>("Sec.MpItms", connect: [permCtxAdjuster, permCheckFactory]), IMultiPermissionCheck
{
    public record Options: MultiPermissionsApp.Options
    {
        public List<IEntity> Entities { get; init; } = [];
    }

    public Dictionary<string, IPermissionCheck> PermissionCheckers
        => field ??= InitializePermissionChecks();


    private Dictionary<string, IPermissionCheck> InitializePermissionChecks()
    {
        return MyOptions.Entities
            .ToDictionary(
                i => i.EntityId.ToString(),
                BuildItemPermissionChecker
            );

        // <summary>
        // Creates a permission checker for an entity in this app
        // </summary>
        IPermissionCheck BuildItemPermissionChecker(IEntity item)
        {
            var l = Log.Fn<IPermissionCheck>($"type: {item.Type}; entity:{item.EntityId}");
            var app = MyOptions.App;

            // user has edit permissions on this app, and it's the same app as the user is coming from
            var ctxSiteOrBlock = permCtxAdjuster.AdjustContextForApp(MyOptions.SiteContext, app);
            var checker = permCheckFactory.ForItem(ctxSiteOrBlock, app, item);

            return l.ReturnAsOk(checker);
        }
    }
}
