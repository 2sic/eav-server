using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Serialization.Sys;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Razor.Blade;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlBase;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsEntity(GenWorkPlus<WorkEntities> workEntities, Generator<JsonSerializer> jsonSerializer, IHttpExceptionMaker exceptionMaker)
    : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [workEntities, jsonSerializer])
{
    public static string Link = "Entity";

    public override string Title => "Entity Details";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, NameId, out var message))
            return message;

        Log.A($"debug app entity metadata for {AppId} and entity {NameId}");
        var entities = workEntities.New(AppId.Value);

        IEntity ent;
        if (int.TryParse(NameId, out var entityId))
            ent = entities.Get(entityId);
        else if (Guid.TryParse(NameId, out var entityGuid))
            ent = entities.Get(entityGuid);
        else
            throw exceptionMaker.BadRequest("can't use entityid - must be number or guid");

        var ser = jsonSerializer.New().SetApp(entities.AppWorkCtx.AppReader);
        var json = ser.Serialize(ent);

        var msg = H1($"Entity Debug for {NameId} in {AppId}\n")
                  + Tags.Nl2Br("\n\n\n")
                  + Textarea(HtmlEncode(json)).Rows("20").Cols("100");

        return msg;
    }

}