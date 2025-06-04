using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsEntityPermissions(GenWorkPlus<WorkEntities> workEntities) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [workEntities])
{
    public static string Link = "EntityPermissions";

    public override string Title => "Entity Permissions";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, NameId, out var message))
            return message;

        var entity = int.Parse(NameId);
        Log.A($"debug app entity permissions for {AppId} and entity {entity}");
        var ent = workEntities.New(AppId.Value).Get(entity);

        var msg = H1($"Entity Permissions for {entity} in {AppId}\n").ToString();
        var permissions = ent.Metadata.Permissions
            .Select(p => ((ICanBeEntity)p).Entity)
            .ToList();

        return MetadataHelper.MetadataTable(msg, AppId.Value, permissions, Linker);
    }


}