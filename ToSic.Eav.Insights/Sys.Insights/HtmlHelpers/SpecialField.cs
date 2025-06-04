namespace ToSic.Eav.Sys.Insights.HtmlHelpers;

internal class SpecialField(object value, string styles = default, string tooltip = default, bool isEncoded = default)
{
    public object Value { get; } = value;

    public string Styles { get; } = styles;

    public string Tooltip { get; } = tooltip;

    public bool IsEncoded { get; } = isEncoded;

    public static SpecialField Center(object value, string tooltip = default, bool isEncoded = default)
        => new(value, "text-align: center;", tooltip: tooltip, isEncoded: isEncoded);

    public static SpecialField Right(object value, string tooltip = default, bool isEncoded = default)
        => new(value, "text-align: right; padding - right: 5px;", tooltip: tooltip, isEncoded: isEncoded);

    public static SpecialField Left(object value, string tooltip = default, bool isEncoded = default)
        => new(value, "text-align: left;", tooltip: tooltip, isEncoded: isEncoded);
}