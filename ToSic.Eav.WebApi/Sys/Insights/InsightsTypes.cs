using ToSic.Eav.Apps.Internal.Insights;
using static ToSic.Razor.Blade.Tag;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsTypes : InsightsProvider
{
    private readonly IAppStates _appStates;

    public InsightsTypes(IAppStates appStates) : base("Types")
    {
        _appStates = appStates;
    }
    

    protected InsightsTypes(IAppStates appStates, string name) : base(name)
    {
        _appStates = appStates;
    }

    public override string Title => $"Content Types for App: {AppId}";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>();
        if (AppId == null)
            return l.Return("please add appid to the url parameters");

        l.A($"debug app types for {AppId}");
        var pkg = _appStates.GetReader(AppId.Value);

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
                    Linker.LinkTo($"{type.Attributes.Count()}", "Attributes", appId, type: type.NameId),
                    Linker.LinkTo($"{type.Metadata.Count()}", "TypeMetadata", appId, type: type.NameId),
                    Linker.LinkTo($"{type.Metadata.Permissions.Count()}", "TypePermissions", appId, type: type.NameId),
                    type.IsDynamic.ToString(),
                    type.RepositoryType.ToString(),
                    Linker.LinkTo($"{itemCount}", "Entities", appId, type: type.NameId)
                ) + "\n";
            }
            msg += "</tbody>" + "\n";
            msg += InsightsHtmlTable.RowFields("", "", "", "", "", "", "", "", "",
                Linker.LinkTo($"{totalItems}", "Entities", appId, type: "all"));
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
}