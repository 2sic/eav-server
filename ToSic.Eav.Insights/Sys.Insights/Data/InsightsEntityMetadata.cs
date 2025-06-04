using ToSic.Eav.Apps.Sys.Work;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsEntityMetadata(GenWorkPlus<WorkEntities> workEntities) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [workEntities])
{
    public static string Link = "EntityMetadata";

    public override string Title => "Entity Metadata";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, NameId, out var message))
            return message;

        var entity = int.Parse(NameId);
        Log.A($"debug app entity metadata for {AppId} and entity {entity}");
        var ent = workEntities.New(AppId.Value).Get(entity);

        var msg = H1($"Entity Metadata for {entity} in {AppId}\n").ToString();
        var metadata = ent.Metadata.ToList();

        return MetadataHelper.MetadataTable(msg, AppId.Value, metadata, Linker);
    }

}