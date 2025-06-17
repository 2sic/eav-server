using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.HtmlHelpers;

public class InsightsHtmlTable: InsightsHtmlBase
{
    internal static IHtmlTag HeadFieldsLeft(object?[] fields)
        => HeadFieldsImplementation(fields
            .Select(f => f as SpecialField ?? SpecialField.Left(f))
            .ToArray()
        );


    internal static IHtmlTag HeadFields(object?[] fields)
        => HeadFieldsImplementation(fields);

    private static IHtmlTag HeadFieldsImplementation(object?[] fields)
        => Thead(
            Tr(
                fields
                    .Where(f => f != null)
                    .Select(fresh => DataToCell(fresh!, true, true)))
        );

    internal static IHtmlTag RowFields(object?[] fields)
        => Tr(
            fields
                .Where(f => f != null)
                .Select(fresh => DataToCell(fresh!, false, false)));

    private static IHtmlTag DataToCell(object fresh, bool encode, bool isHeader)
    {
        var data = fresh;
        var special = fresh as SpecialField;
        if (special != null)
            data = special.Value;

        var contents = (data ?? "").ToString();
        if (encode && special?.IsEncoded != true)
            contents = HtmlEncode(contents);
        var cell = isHeader
            ? Th(contents) as IHtmlTag
            : Td(contents);

        if (special?.Styles != null)
            cell.Style(special.Styles);
        if (special?.Tooltip != null)
            cell.Title(special.Tooltip);
        return cell;
    }
}