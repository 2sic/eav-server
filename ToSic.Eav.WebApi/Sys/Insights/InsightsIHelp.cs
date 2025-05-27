using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Lib.Coding;
using ToSic.Razor.Html5;
using ToSic.Sys.Utils;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

/// <summary>
/// Home / Help screen for insights
/// </summary>
/// <param name="insightsProviders">All providers - MUST BE LAZY - otherwise we get circular DI dependencies</param>
internal class InsightsHelp(LazySvc<IEnumerable<IInsightsProvider>> insightsProviders)
    : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay)
{
    public static string Link = "Help";

    public override string Title => "Insights Help / Home";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>();

        // ReSharper disable IdentifierTypo
        // ReSharper disable StringLiteralTypo
        const string typeattribs = "typeattributes?appid=&type=";
        const string typeMeta = "typemetadata?appid=&type=";
        const string typePerms = "typepermissions?appid=&type=";
        const string attribMeta = "attributemetadata?appid=&type=&attribute=";
        const string attribPerms = "attributepermissions?appid=&type=&attribute=";
        // ReSharper restore IdentifierTypo
        // ReSharper restore StringLiteralTypo

        var providersWithCategory = insightsProviders.Value
            // Skip self
            .Where(p => !p.Name.EqualsInsensitive(Name))
            // Skip hidden - these are usually sub-providers which require additional parameters
            .Where(p => p.HelpCategory != HiddenFromAutoDisplay)
            // Group by category - if not set, use a default
            .GroupBy(p => p.HelpCategory ?? "Uncategorized (please add Category)")
            .OrderBy(g => g.Key)
            .ToList();

        var extras = providersWithCategory
            .Select(g =>
                H2(g.Key)
                + Ol(g.Select(p =>
                        Li(
                            LinkTo(p.Title, p.Name),
                            p.Teaser.HasValue()
                                ? $"<br>{p.Teaser}"
                                : ""
                        )
                    )
                )
            )
            .ToList();

        var result = RawHtml(
                H1("2sxc Insights - Commands"),
                P(
                    "In most cases you'll just browse the cache and use the links from there. "
                    + "The other links are listed here so you know what they would be, "
                    + "in case something is preventing you from browsing the normal way. "
                    + "Read more about 2sxc insights in the "
                    + A("blog post").Href("https://2sxc.org/en/blog/post/using-2sxc-insights").Target("_blank")
                ),

                H2("Most used"),
                Ol(
                    Li(LinkTo("Help (this screen", InsightsHelp.Link)),
                    Li(LinkTo("All Logs", InsightsLogs.Link)),
                    Li(LinkTo("In Memory Cache", InsightsAppsCache.Link)),
                    Li(LinkTo("In Memory DataSource Cache", nameof(InsightsDataSourceCache.DataSourceCache))),
                    Li(LinkTo("ping the system / IsAlive", ProviderName(nameof(InsightsIsAlive))))
                ),

                H2("Global Data &amp; Types"),
                Ol(
                    Li(LinkTo("Global Types in cache", ProviderName(nameof(InsightsGlobalTypes)))),
                    Li(LinkTo("Global Types loading log", InsightsGlobalTypesLog.Link)),
                    Li(LinkTo("Global logs", InsightsLogs.Link, key: LogNames.LogStoreStartUp)),
                    Li(LinkTo("Licenses &amp; Features", InsightsLicenses.Link))
                ),
                RawHtml(extras.Cast<object>().ToArray()),

                H2("Manual links to access debug information"),
                Ol(
                    Li("flush an app cache: " + DemoLink("purge?appid=")),
                    Li(
                        $"look at the load-log of an app-cache: <a href='{InsightsAppLoadLog.Link}?appid='>{InsightsAppLoadLog.Link}?appid=</a>"),
                    Li(
                        $"look at the cache-stats of an app: <a href='{InsightsAppStats.Link}?appid='>{InsightsAppStats.Link}?appid=</a>"),
                    Li(
                        $"look at the content-types of an app: <a href='{ProviderName(nameof(InsightsTypes))}?appid='>{ProviderName(nameof(InsightsTypes))}?appid=</a>"),
                    Li("look at attributes of a type: " + DemoLink(typeattribs)),
                    Li("look at type metadata:" + DemoLink(typeMeta)),
                    Li("look at type permissions:" + DemoLink(typePerms)),
                    Li("look at attribute Metadata :" + DemoLink(attribMeta)),
                    Li("look at attribute permissions:" + DemoLink(attribPerms)),
                    Li("look at entities of a type:" + DemoLink($"{InsightsEntities.Link}?appid=&type=")),
                    Li("look at all entities:" + DemoLink($"{InsightsEntities.Link}?appid=&type=all")),
                    Li("look at a single entity by id:" + DemoLink($"{InsightsEntity.Link}?appId=&entity=")),
                    Li("look at entity metadata using entity-id:" +
                       DemoLink($"{InsightsEntityMetadata.Link}?appid=&entity=")),
                    Li("look at entity permissions using entity-id:" +
                       DemoLink($"{InsightsEntityPermissions.Link}?appid=&entity=")
                    )
                ))
            ;
        return l.ReturnAsOk(result.ToString());
    }

    private static string ProviderName(string longName)
        => longName.Replace("Insights", "").Replace("Provider", "").Trim();

    private InsightsHtmlTable HtmlTableBuilder { get; } = new();

    internal A DemoLink(string labelAndLink) => HtmlTableBuilder.DemoLink(labelAndLink);

    internal A LinkTo(string label, string view,
        int? appId = null, NoParamOrder noParamOrder = default,
        string key = null, string type = null, string nameId = null, string more = null)
    {
        return HtmlTableBuilder.LinkTo(label, view, appId, noParamOrder, key, type, nameId, more);
    }

}