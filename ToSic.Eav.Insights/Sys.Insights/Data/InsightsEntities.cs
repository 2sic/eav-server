﻿using System.Globalization;
using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlBase;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsEntities(GenWorkPlus<WorkEntities> workEntities)
    : InsightsProvider(new() { Name = Link, Title = "Entities" }, connect: [workEntities])
{
    public static string Link = "Entities";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        Log.A($"debug app attributes for {AppId} and {Type}");
        var appEntities = workEntities.New(AppId ?? 0);

        var typ = appEntities.AppWorkCtx.AppReader.TryGetContentType(Type);

        var msg = "" + H1($"Entities of {Type} ({HtmlEncode(typ?.Name)}/{typ?.NameId}) in {AppId}\n");
        try
        {
            Log.A("getting content-type stats");
            var entities = Type == "all"
                ? appEntities.All().ToListOpt()
                : appEntities.Get(Type).ToListOpt();
            msg += P($"entities: {entities.Count}\n");
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields(["#", "Id", AttributeNames.GuidNiceName, AttributeNames.TitleNiceName, "Type", "Modified", "Owner", "Version", "Metadata", "Permissions"])
                   + "<tbody>";
            var count = 0;
            foreach (var ent in entities)
            {
                msg += InsightsHtmlTable.RowFields([
                    (++count).ToString(),
                    Linker.LinkTo($"{ent.EntityId}", InsightsEntity.Link, AppId, nameId: ent.EntityId.ToString()),
                    Linker.LinkTo($"{ent.EntityGuid}", InsightsEntity.Link, AppId, nameId: ent.EntityGuid.ToString()),
                    ent.GetBestTitle(),
                    HtmlEncode(ent.Type.Name),
                    ent.Modified.ToString(CultureInfo.InvariantCulture),
                    ent.Owner,
                    $"{ent.Version}",
                    Linker.LinkTo($"{ent.Metadata.Count()}", InsightsEntityMetadata.Link, AppId, nameId: ent.EntityId.ToString()),
                    Linker.LinkTo($"{ent.Metadata.Permissions.Count()}", InsightsEntityPermissions.Link, AppId, nameId: ent.EntityId.ToString())
                ]);
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