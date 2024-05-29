using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Lib.Coding;
using ToSic.Razor.Html5;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

partial class InsightsControllerReal
{
    internal string Help()
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

        var providersWithCategory = insightsProviders
            // Skip self
            .Where(p => !p.Name.EqualsInsensitive(nameof(Help)))
            // Skip hidden - these are usually sub-providers which require additional parameters
            .Where(p => p.HelpCategory != InsightsProvider.HiddenFromAutoDisplay)
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
                    Li(LinkTo("Help (this screen", nameof(Help))),
                    Li(LinkTo("All Logs", nameof(Logs))),
                    Li(LinkTo("In Memory Cache", nameof(Cache))),
                    Li(LinkTo("In Memory DataSource Cache", nameof(dsCache.Value.DataSourceCache))),
                    Li(LinkTo("ping the system / IsAlive", ProviderName(nameof(InsightsIsAlive))))
                ),

                H2("Global Data &amp; Types"),
                Ol(
                    Li(LinkTo("Global Types in cache", ProviderName(nameof(InsightsGlobalTypes)))),
                    Li(LinkTo("Global Types loading log", nameof(GlobalTypesLog))),
                    Li(LinkTo("Global logs", nameof(Logs), key: LogNames.LogStoreStartUp)),
                    Li(LinkTo("Licenses &amp; Features", nameof(Licenses)))
                ),
                RawHtml(extras.Cast<object>().ToArray()),

                H2("Manual links to access debug information"),
                Ol(
                    Li("flush an app cache: " + DemoLink("purge?appid=")),
                    Li(
                        $"look at the load-log of an app-cache: <a href='{nameof(LoadLog)}?appid='>{nameof(LoadLog)}?appid=</a>"),
                    Li(
                        $"look at the cache-stats of an app: <a href='{nameof(Stats)}?appid='>{nameof(Stats)}?appid=</a>"),
                    Li(
                        $"look at the content-types of an app: <a href='{ProviderName(nameof(InsightsTypes))}?appid='>{ProviderName(nameof(InsightsTypes))}?appid=</a>"),
                    Li("look at attributes of a type: " + DemoLink(typeattribs)),
                    Li("look at type metadata:" + DemoLink(typeMeta)),
                    Li("look at type permissions:" + DemoLink(typePerms)),
                    Li("look at attribute Metadata :" + DemoLink(attribMeta)),
                    Li("look at attribute permissions:" + DemoLink(attribPerms)),
                    Li("look at entities of a type:" + DemoLink($"{nameof(Entities)}?appid=&type=")),
                    Li("look at all entities:" + DemoLink($"{nameof(Entities)}?appid=&type=all")),
                    Li("look at a single entity by id:" + DemoLink($"{nameof(Entity)}?appId=&entity=")),
                    Li("look at entity metadata using entity-id:" +
                       DemoLink($"{nameof(EntityMetadata)}?appid=&entity=")),
                    Li("look at entity permissions using entity-id:" +
                       DemoLink($"{nameof(EntityPermissions)}?appid=&entity=")
                    )
                ))
            ;
        return l.ReturnAsOk(result.ToString());
    }

    private static string ProviderName(string longName) => longName.Replace("Insights", "").Replace("Provider", "").Trim();

    internal A DemoLink(string labelAndLink) => HtmlTableBuilder.DemoLink(labelAndLink);
        
    internal A LinkTo(string label, string view, 
        int? appId = null, NoParamOrder noParamOrder = default,
        string key = null, string type = null, string nameId = null, string more = null)
    {
        return HtmlTableBuilder.LinkTo(label, view, appId, noParamOrder, key, type, nameId,more);
    }

}