using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsAttributes(LazySvc<IAppStates> appStates) : InsightsProviderWeb(Link, helpCategory: HiddenFromAutoDisplay, connect: [appStates])
{
    public static string Link = "Attributes";

    public override string Title => "Attributes of Type";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        Log.A($"debug app attributes for {AppId} and {Type}");
        var typ = appStates.Value.GetReader(AppId.Value).GetContentType(Type);

        var msg = "" + H1($"Attributes for {typ.Name} ({typ.NameId}) in {AppId}\n");
        try
        {
            Log.A("getting content-type stats");
            var attribs = typ.Attributes;
            msg += P($"attribs: {attribs.Count()}\n");
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields("#", "Id", "Name", "Type", "Input", "IsTitle", "Metadata", "Permissions")
                   + "<tbody>";
            var count = 0;
            foreach (var att in attribs)
            {
                msg += InsightsHtmlTable.RowFields(
                    (++count).ToString(),
                    att.AttributeId.ToString(),
                    att.Name,
                    att.Type.ToString(),
                    att.InputType(),
                    InsightsHtmlBase.EmojiTrueFalse(att.IsTitle),
                    Linker.LinkTo($"{att.Metadata.Count()}", InsightsAttributeMetadata.Link, AppId, type: Type, nameId: att.Name),
                    Linker.LinkTo($"{att.Metadata.Permissions.Count()}", InsightsAttributePermissions.Link, AppId, type: Type, nameId: att.Name)
                );
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