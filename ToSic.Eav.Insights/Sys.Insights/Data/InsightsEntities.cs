using System.Collections.Immutable;
using System.Globalization;
using ToSic.Eav.Apps.Internal.Insights;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsEntities(GenWorkPlus<WorkEntities> workEntities) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [workEntities])
{
    public static string Link = "Entities";

    public override string Title => "Entities";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        Log.A($"debug app attributes for {AppId} and {Type}");
        var appEntities = workEntities.New(AppId ?? 0);

        var typ = appEntities.AppWorkCtx.AppReader.GetContentType(Type);

        var msg = "" + H1($"Entities of {Type} ({HtmlEncode(typ?.Name)}/{typ?.NameId}) in {AppId}\n");
        try
        {
            Log.A("getting content-type stats");
            var entities = Type == "all"
                ? appEntities.All().ToImmutableList()
                : appEntities.Get(Type).ToImmutableList();
            msg += P($"entities: {entities.Count}\n");
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields("#", "Id", AttributeNames.GuidNiceName, Eav.Data.AttributeNames.TitleNiceName, "Type", "Modified", "Owner", "Version", "Metadata", "Permissions")
                   + "<tbody>";
            var count = 0;
            foreach (var ent in entities)
            {
                msg += InsightsHtmlTable.RowFields(
                    (++count).ToString(),
                    Linker.LinkTo($"{ent.EntityId}", InsightsEntity.Link, AppId, nameId: ent.EntityId.ToString()),
                    Linker.LinkTo($"{ent.EntityGuid}", InsightsEntity.Link, AppId, nameId: ent.EntityGuid.ToString()),
                    ent.GetBestTitle(),
                    HtmlEncode(ent.Type.Name),
                    ent.Modified.ToString(CultureInfo.InvariantCulture),
                    ent.Owner,
                    $"{ent.Version}",
                    Linker.LinkTo($"{ent.Metadata.Count()}", InsightsEntityMetadata.Link, AppId, nameId: ent.EntityId.ToString()),
                    Linker.LinkTo($"{ent.Metadata.Permissions.Count()}", InsightsEntityPermissions.Link, AppId, nameId: ent.EntityId.ToString()));
            }
            msg += "</tbody>";
            msg += "</table>";
            msg += "\n\n";
            msg += InsightsHtmlParts.JsTableSort();
        }
        catch
        {
            // ignored
        }

        return msg;
    }

}