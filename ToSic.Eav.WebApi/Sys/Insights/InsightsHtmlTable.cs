using System.Linq;
using ToSic.Razor.Blade;
using ToSic.Razor.Html5;
using ToSic.Razor.Markup;
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
                        .Select(fresh =>
                        {
                            return DataToCell(fresh, true, true);
                            // return Th(HtmlEncode((fresh ?? "").ToString()));
                        })
                        // .ToArray<object>()
                )
            );

        internal static IHtmlTag RowFields(params object[] fields)
            => Tr(
                fields
                    .Where(f => f != null)
                    .Select(fresh => DataToCell(fresh, false, false))
                    // .ToArray<object>()
                );

        private static IHtmlTag DataToCell(object fresh, bool encode, bool isHeader)
        {
            var data = fresh;
            string styles = null;
            if (fresh is SpecialField special)
            {
                data = special.Value;
                styles = special.Styles;
            }

            var contents = (data ?? "").ToString();
            if (encode) contents = HtmlEncode(contents);
            var cell = isHeader ? Th(contents) as IHtmlTag : Td(contents);
            if (styles != null)
                cell.Style(styles);
            return cell;
        }
    }
}
