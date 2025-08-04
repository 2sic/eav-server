using System.Text;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using static ToSic.Razor.Blade.Tag;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlBase;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsTypes(IAppReaderFactory appReadFac, string name, string? title = default) // TODO: [AppId]
    : InsightsProvider(new() { Name = name, Title = title ?? "Content Types for App: [AppId]" })
{
    public static string Link = "Types";

    protected readonly IAppReaderFactory AppReadFac = appReadFac;

    public InsightsTypes(IAppReaderFactory appReadFac) : this(appReadFac, "Types") { }

    public override string HtmlBody()
    {
        var l = Log.Fn<string>();
        if (AppId == null)
            return l.Return("please add appid to the url parameters");

        l.A($"debug app types for {AppId}");
        var pkg = AppReadFac.Get(AppId.Value);

        var msg = TypesTable(AppId.Value, pkg.ContentTypes, pkg.List);

        return l.ReturnAsOk(msg);
    }

    internal string TypesTable(int appId, IEnumerable<IContentType> typesA, IReadOnlyCollection<IEntity> items)
    {
        var l = Log.Fn<string>($"appId:{appId}");

        var html = new StringBuilder();
        try
        {
            l.A("getting content-type stats");
            var types = typesA
                .OrderBy(t => t.RepositoryType)
                .ThenBy(t => t.Scope)
                .ThenBy(t => t.NameId)
                .ToList();
            html.AppendLine(P($"types: {types.Count}\n") + "");
            html.AppendLine("<table id='table'>"
                   + InsightsHtmlTable.HeadFields([
                       "#", "Scope", "StaticName", "Name", "Attribs", "Metadata", "Permissions", "IsDyn",
                       "Repo", "Items"
                   ])
                   + "<tbody>"
                   + "\n");
            var totalItems = 0;
            var count = 0;

            var listOfItemsFound = new List<IEntity>();
            foreach (var type in types)
            {
                var itemCount = 0;
                try
                {
                    var ofType = items
                        .Where(e => Equals(e.Type, type))
                        .ToListOpt();
                    itemCount = ofType.Count;
                    totalItems += itemCount;
                    listOfItemsFound.AddRange(ofType);
                }
                catch
                {
                    /*ignore*/
                }

                html.AppendLine(InsightsHtmlTable.RowFields([
                    ++count,
                    type.Scope,
                    type.NameId,
                    HtmlEncode(type.Name),
                    //type.Name,
                    Linker.LinkTo($"{type.Attributes.Count()}", nameof(AttributeNames), appId, type: type.NameId),
                    Linker.LinkTo($"{type.Metadata.Count()}", InsightsTypeMetadata.Link, appId, type: type.NameId),
                    Linker.LinkTo($"{type.Metadata.Permissions.Count()}", InsightsTypePermissions.Link, appId, type: type.NameId),
                    type.IsDynamic.ToString(),
                    type.RepositoryType.ToString(),
                    Linker.LinkTo($"{itemCount}", "Entities", appId, type: type.NameId)
                ]) + "");
            }

            // Find items which are not properly found by type
            var unhandled = items
                .Where(i => !listOfItemsFound.Contains(i))
                .ToListOpt();

            var unhandledGrouped = unhandled
                .GroupBy(e => e.Type.NameId)
                .ToListOpt();

            foreach (var entityGroup in unhandledGrouped)
            {
                var type = entityGroup.First().Type;
                html.AppendLine(InsightsHtmlTable.RowFields([
                    ++count,
                    type.Scope,
                    type.NameId + HtmlEncode(" ⚠️ not found"),
                    HtmlEncode(type.Name),
                    //type.Name,
                    Linker.LinkTo($"{type.Attributes.Count()}", nameof(AttributeNames), appId, type: type.NameId),
                    Linker.LinkTo($"{type.Metadata.Count()}", InsightsTypeMetadata.Link, appId, type: type.NameId),
                    Linker.LinkTo($"{type.Metadata.Permissions.Count()}", InsightsTypePermissions.Link, appId, type: type.NameId),
                    type.IsDynamic.ToString(),
                    type.RepositoryType.ToString(),
                    Linker.LinkTo($"{entityGroup.Count()}", "Entities", appId, type: type.NameId)
                ]) + "");

            }

            html.AppendLine("</tbody>" + "\n");
            html.AppendLine(InsightsHtmlTable.RowFields([
                    "", "", "", "", "", "", "", "", "",
                    Linker.LinkTo($"{totalItems}", "Entities", appId, type: "all")
                ]) + ""
            );
            html.AppendLine("</table>");
            html.AppendLine("\n\n");
            html.AppendLine(P($"Total item in system: {items.Count} - " +
                                   $"in types: {totalItems} - " +
                                   $"numbers {Em("should")} match! - " +
                                   $"delta is {unhandled.Count} {HtmlEncode(unhandled.Count == 0 ? "✅" : "⚠️")}") + ""
            );
            html.AppendLine(InsightsHtmlParts.JsTableSort() + "");
        }
        catch (Exception ex)
        {
            l.Ex(ex);
        }

        return l.ReturnAsOk(html.ToString());
    }
}