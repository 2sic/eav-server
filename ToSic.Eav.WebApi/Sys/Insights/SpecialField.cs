namespace ToSic.Eav.WebApi.Sys.Insights;

internal class SpecialField(object value, string styles = default, string tooltip = default)
{
    public object Value { get; } = value;
    public string Styles { get; } = styles;

    public string Tooltip { get; } = tooltip;

    public static SpecialField Center(object value) => new(value, "text-align: center;");

    public static SpecialField Right(object value) => new(value, "text-align: right; padding - right: 5px;");

    public static SpecialField Left(object value) => new(value, "text-align: left;");
}