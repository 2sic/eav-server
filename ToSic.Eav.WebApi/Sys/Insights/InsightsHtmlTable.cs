using System.Linq;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights
{
    internal class InsightsHtmlTable: InsightsHtmlBase
    {
        internal static IHtmlTag HeadFields(params object[] fields)
            => Thead(
                Tr(
                    fields
                        .Where(f => f != null)
                        .Select(fresh => Th(HtmlEncode((fresh ?? "").ToString())))
                        .ToArray<object>()
                )
            );

        internal static IHtmlTag RowFields(params object[] fields)
            => Tr(
                fields
                    .Where(f => f != null)
                    .Select(fresh =>
                    {
                        var data = fresh;
                        string styles = null;
                        if (fresh is SpecialField special)
                        {
                            data = special.Value;
                            styles = special.Styles;
                        }

                        var td = Td((data ?? "").ToString());
                        if (styles != null)
                            td.Style(styles);
                        return td;
                    })
                    .ToArray<object>());

    }
}
