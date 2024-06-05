using System.Collections.Immutable;
using System.Globalization;
using ToSic.Eav.Serialization.Internal;
using ToSic.Razor.Blade;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

partial class InsightsControllerReal
{

    private string Entities(int? appId, string type)
    {
        if (UrlParamsIncomplete(appId, type, out var message))
            return message;

        Log.A($"debug app attributes for {appId} and {type}");
        var appEntities = workEntities.New(appId ?? 0);

        var typ = appEntities.AppWorkCtx.AppState.GetContentType(type);

        var msg = "" + H1($"Entities of {type} ({HtmlEncode(typ?.Name)}/{typ?.NameId}) in {appId}\n");
        try
        {
            Log.A("getting content-type stats");
            var entities = type == "all"
                ? appEntities.All().ToImmutableList()
                : appEntities.Get(type).ToImmutableList();
            msg += P($"entities: {entities.Count}\n");
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields("#", "Id", Eav.Data.Attributes.GuidNiceName, Eav.Data.Attributes.TitleNiceName, "Type", "Modified", "Owner", "Version", "Metadata", "Permissions")
                   + "<tbody>";
            var count = 0;
            foreach (var ent in entities)
            {
                msg += InsightsHtmlTable.RowFields(
                    (++count).ToString(),
                    LinkTo($"{ent.EntityId}", nameof(Entity), appId, nameId: ent.EntityId.ToString()),
                    LinkTo($"{ent.EntityGuid}", nameof(Entity), appId, nameId: ent.EntityGuid.ToString()),
                    ent.GetBestTitle(),
                    HtmlEncode(ent.Type.Name),
                    ent.Modified.ToString(CultureInfo.InvariantCulture),
                    ent.Owner,
                    $"{ent.Version}",
                    LinkTo($"{ent.Metadata.Count()}", nameof(EntityMetadata), appId, nameId: ent.EntityId.ToString()),
                    LinkTo($"{ent.Metadata.Permissions.Count()}", nameof(EntityPermissions), appId, nameId: ent.EntityId.ToString()));
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

    private string EntityMetadata(int? appId = null, int? entity = null)
    {
        if (UrlParamsIncomplete(appId, entity, out var message))
            return message;

        Log.A($"debug app entity metadata for {appId} and entity {entity}");
        var ent = workEntities.New(appId.Value).Get(entity.Value);

        var msg = H1($"Entity Metadata for {entity} in {appId}\n").ToString();
        var metadata = ent.Metadata.ToList();

        return InsightsMetadataHelper.MetadataTable(msg, metadata);
    }

    private string EntityPermissions(int? appId = null, int? entity = null)
    {
        if (UrlParamsIncomplete(appId, entity, out var message))
            return message;

        Log.A($"debug app entity permissions for {appId} and entity {entity}");
        var ent = workEntities.New(appId.Value).Get(entity.Value);

        var msg = H1($"Entity Permissions for {entity} in {appId}\n").ToString();
        var permissions = ent.Metadata.Permissions.Select(p => p.Entity).ToList();

        return InsightsMetadataHelper.MetadataTable(msg, permissions);
    }

    private string Entity(int? appId, string nameId)
    {
        if (UrlParamsIncomplete(appId, nameId, out var message))
            return message;

        Log.A($"debug app entity metadata for {appId} and entity {nameId}");
        var entities = workEntities.New(appId.Value);

        IEntity ent;
        if (int.TryParse(nameId, out var entityId))
            ent = entities.Get(entityId);
        else if (Guid.TryParse(nameId, out var entityGuid))
            ent = entities.Get(entityGuid);
        else
            throw CreateBadRequest("can't use entityid - must be number or guid");

        var ser = jsonSerializer.New().SetApp(entities.AppWorkCtx.AppState);
        var json = ser.Serialize(ent);

        var msg = H1($"Entity Debug for {nameId} in {appId}\n")
                  + Tags.Nl2Br("\n\n\n")
                  + Textarea(HtmlEncode(json)).Rows("20").Cols("100");

        return msg;
    }

}