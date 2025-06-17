using ToSic.Eav.Data.Sys;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using static ToSic.Razor.Blade.Tag;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlBase;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsTypes(IAppReaderFactory appReadFac, string name, string? title = default) // TODO: [aPPiD]
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

        var msg = "";
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
                   + InsightsHtmlTable.HeadFields([
                       "#", "Scope", "StaticName", "Name", "Attribs", "Metadata", "Permissions", "IsDyn",
                       "Repo", "Items"
                   ])
                   + "<tbody>"
                   + "\n";
            var totalItems = 0;
            var count = 0;
            foreach (var type in types)
            {
                var itemCount = 0;
                try
                {
                    var itms = items
                        .Where(e => Equals(e.Type, type))
                        .ToListOpt();
                    itemCount = itms.Count;
                    totalItems += itemCount;
                }
                catch
                {
                    /*ignore*/
                }

                msg += InsightsHtmlTable.RowFields([
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
                ]) + "\n";
            }
            msg += "</tbody>" + "\n";
            msg += InsightsHtmlTable.RowFields(["", "", "", "", "", "", "", "", "",
                Linker.LinkTo($"{totalItems}", "Entities", appId, type: "all")]);
            msg += "</table>";
            msg += "\n\n";
            msg += P(
                $"Total item in system: {items.Count} - in types: {totalItems} - numbers {Em("should")} match!");
            msg += InsightsHtmlParts.JsTableSort();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
        }

        return l.ReturnAsOk(msg);
    }
}