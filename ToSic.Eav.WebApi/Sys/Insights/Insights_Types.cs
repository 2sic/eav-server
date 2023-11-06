using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.File;
using static ToSic.Razor.Blade.Tag;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;

namespace ToSic.Eav.WebApi.Sys.Insights
{
    public partial class InsightsControllerReal
    {
        private string Types(int? appId = null)
        {
            var l = Log.Fn<string>();
            if (appId == null)
                return l.Return("please add appid to the url parameters");

            l.A($"debug app types for {appId}");
            var pkg = _appStates.Get(appId.Value);

            var msg = TypesTable(appId.Value, pkg.ContentTypes, pkg.List);

            return l.ReturnAsOk(msg);
        }

        private string TypesTable(int appId, IEnumerable<IContentType> typesA, IReadOnlyCollection<IEntity> items)
        {
            var l = Log.Fn<string>($"appId:{appId}");

            var msg = H1($"App types for {appId}\n").ToString();
            try
            {
                l.A("getting content-type stats");
                var types = typesA
                    .OrderBy(t => t.RepositoryType)
                    .ThenBy(t => t.Scope)
                    .ThenBy(t => t.NameId)
                    .ToList();
                msg += P($"types: {types.Count}\n");
                msg += "<table id='table'>"
                       + InsightsHtmlTable.HeadFields(
                               "#", "Scope", "StaticName", "Name", "Attribs", "Metadata", "Permissions", "IsDyn",
                               "Repo", "Items"
                           )
                       + "<tbody>"
                       + "\n";
                var totalItems = 0;
                var count = 0;
                foreach (var type in types)
                {
                    int? itemCount = null;
                    try
                    {
                        var itms = items?.Where(e => e.Type == type);
                        itemCount = itms?.Count();
                        totalItems += itemCount ?? 0;
                    }
                    catch
                    {
                        /*ignore*/
                    }

                    msg += InsightsHtmlTable.RowFields(
                        ++count,
                        type.Scope,
                        type.NameId,
                        HtmlEncode(type.Name),
                        //type.Name,
                        LinkTo($"{type.Attributes.Count()}", nameof(Attributes), appId, type: type.NameId),
                        LinkTo($"{type.Metadata.Count()}", nameof(TypeMetadata), appId, type: type.NameId),
                        LinkTo($"{type.Metadata.Permissions.Count()}", nameof(TypePermissions), appId, type: type.NameId),
                        type.IsDynamic.ToString(),
                        type.RepositoryType.ToString(),
                        LinkTo($"{itemCount}", nameof(Entities), appId, type: type.NameId)
                    ) + "\n";
                }
                msg += "</tbody>" + "\n";
                msg += InsightsHtmlTable.RowFields("", "", "", "", "", "", "", "", "",
                    LinkTo($"{totalItems}", nameof(Entities), appId, type: "all"));
                msg += "</table>";
                msg += "\n\n";
                msg += P(
                    $"Total item in system: {items?.Count} - in types: {totalItems} - numbers {Em("should")} match!");
                msg += InsightsHtmlParts.JsTableSort();
            }
            catch (Exception ex)
            {
                l.Ex(ex);
            }

            return l.ReturnAsOk(msg);
        }

        private string GlobalTypes()
        {
            var globTypes = _appStates.GetPresetApp().ContentTypes;
            return TypesTable(Eav.Constants.PresetAppId, globTypes, null);
        }

        private string GlobalTypesLog()
        {
            var msg = InsightsHtmlParts.PageStyles() + _logHtml.LogHeader(null, false);
            var log = Runtime.LoadLog;
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
            var typ = AppState(appId).GetContentType(type);

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
            var typ = AppState(appId).GetContentType(type);

            var msg = H1($"Permissions for {typ.Name} ({typ.NameId}) in {appId}\n").ToString();
            var metadata = typ.Metadata.Permissions.Select(p => p.Entity).ToList();

            return l.ReturnAsOk(MetadataTable(msg, metadata));
        }

    }
}
