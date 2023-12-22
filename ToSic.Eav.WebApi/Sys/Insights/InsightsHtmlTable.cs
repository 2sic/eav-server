using System.Linq;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

public class InsightsHtmlTable: InsightsHtmlBase
{
    internal static IHtmlTag HeadFieldsLeft(params object[] fields) 
        => HeadFieldsImplementation(fields.Select(f => f is SpecialField fs ? fs : SpecialField.Left(f)).ToArray());


    internal static IHtmlTag HeadFields(params object[] fields)
        => HeadFieldsImplementation(fields);

    private static IHtmlTag HeadFieldsImplementation(object[] fields)
        => Thead(
            Tr(
                fields
                    .Where(f => f != null)
                    .Select(fresh => DataToCell(fresh, true, true)))
        );

    internal static IHtmlTag RowFields(params object[] fields)
        => Tr(
            fields
                .Where(f => f != null)
                .Select(fresh => DataToCell(fresh, false, false)));

    private static IHtmlTag DataToCell(object fresh, bool encode, bool isHeader)
    {
        var data = fresh;
        var special = fresh as SpecialField;
        //var styles = special?.Styles;
        if (special != null)
        {
            data = special.Value;
            //styles = special.Styles;
        }

        var contents = (data ?? "").ToString();
        if (encode) contents = InsightsHtmlBase.HtmlEncode(contents);
        var cell = isHeader ? Th(contents) as IHtmlTag : Td(contents);
        if (special?.Styles != null)
            cell.Style(special.Styles);
        if (special?.Tooltip != null)
            cell.Title(special.Tooltip);
        return cell;
    }
}