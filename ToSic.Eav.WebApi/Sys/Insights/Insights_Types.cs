using ToSic.Eav.Persistence.File;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

partial class InsightsControllerReal
{

    private string GlobalTypesLog()
    {
        var msg = InsightsHtmlParts.PageStyles() + _logHtml.LogHeader(null, false);
        var log = InternalAppLoader.LoadLog;
        return msg + (log == null
            ? P("log is null").ToString()
            : _logHtml.DumpTree("Log for Global Types loading", log));
    }

    private string TypeMetadata(int? appId = null, string type = null)
    {
        var l = Log.Fn<string>($"appId:{appId}");
            
        if (UrlParamsIncomplete(appId, type, out var message))
            return message;

        l.A($"debug app metadata for {appId} and {type}");
        var typ = AppState(appId.Value).GetContentType(type);

        var msg = H1($"Metadata for {typ.Name} ({typ.NameId}) in {appId}\n").ToString();
        var metadata = typ.Metadata.ToList();

        return l.ReturnAsOk(MetadataTable(msg, metadata));
    }

    private string TypePermissions(int? appId = null, string type = null)
    {
        var l = Log.Fn<string>($"appId:{appId}");
            
        if (UrlParamsIncomplete(appId, type, out var message))
            return message;

        l.A($"debug app metadata for {appId} and {type}");
        var typ = AppState(appId.Value).GetContentType(type);

        var msg = H1($"Permissions for {typ.Name} ({typ.NameId}) in {appId}\n").ToString();
        var metadata = typ.Metadata.Permissions.Select(p => p.Entity).ToList();

        return l.ReturnAsOk(MetadataTable(msg, metadata));
    }

}