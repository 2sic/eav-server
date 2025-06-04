using ToSic.Eav.Apps.Internal.Insights;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class MetadataHelper
{

    internal static string MetadataTable(string msg, int appId, List<IEntity> metadata, IInsightsLinker linker)
    {
        try
        {
            msg += P($"Assigned Items: {metadata.Count}\n");
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields( "#", "Id", Eav.Data.AttributeNames.TitleNiceName, "Content-Type", "Target", "Key" )
                   + "<tbody>";
            var count = 0;
            foreach (var md in metadata)
            {
                var mdFor = md.MetadataFor;
                var key = !string.IsNullOrEmpty(mdFor.KeyString)
                    ? "\"" + mdFor.KeyString + "\""
                    : mdFor.KeyNumber != null
                        ? "#" + mdFor.KeyNumber
                        : mdFor.KeyGuid != null
                            ? "{" + mdFor.KeyGuid + "}"
                            : "(directly attached)";

                msg += InsightsHtmlTable.RowFields(
                    ++count,
                    linker.LinkTo($"{md.EntityId}", InsightsEntity.Link, appId, nameId: md.EntityId.ToString()),
                    md.EntityId,
                    md.GetBestTitle(),
                    linker.LinkTo(md.Type.Name, InsightsEntities.Link, appId, type: md.Type.Name),
                    // md.Type.Name,
                    mdFor.TargetType,
                    key
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