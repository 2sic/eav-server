﻿using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsAttributes(LazySvc<IAppReaderFactory> appReaders)
    : InsightsProvider(new() { Name = Link, Title = "Attributes of Type"}, connect: [appReaders])
{
    public static string Link = "Attributes";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        Log.A($"debug app attributes for {AppId} and {Type}");
        var typ = appReaders.Value.Get(AppId.Value).GetContentType(Type);

        var msg = "" + H1($"Attributes for {typ.Name} ({typ.NameId}) in {AppId}\n");
        try
        {
            Log.A("getting content-type stats");
            var attribs = typ.Attributes.ToListOpt();
            msg += P($"attribs: {attribs.Count()}\n");
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields(["#", "Id", "Name", "Type", "Input", "IsTitle", "Metadata", "Permissions"])
                   + "<tbody>";
            var count = 0;
            foreach (var att in attribs)
            {
                msg += InsightsHtmlTable.RowFields([
                    (++count).ToString(),
                    att.AttributeId.ToString(),
                    att.Name,
                    att.Type.ToString(),
                    att.InputType(),
                    InsightsHtmlBase.EmojiTrueFalse(att.IsTitle),
                    Linker.LinkTo($"{att.Metadata.Count()}", InsightsAttributeMetadata.Link, AppId, type: Type, nameId: att.Name),
                    Linker.LinkTo($"{att.Metadata.Permissions.Count()}", InsightsAttributePermissions.Link, AppId, type: Type, nameId: att.Name)
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