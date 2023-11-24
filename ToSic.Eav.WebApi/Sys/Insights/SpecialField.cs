﻿namespace ToSic.Eav.WebApi.Sys.Insights;

internal class SpecialField
{

    public SpecialField(object value, string styles = default, string tooltip = default)
    {
        Styles = styles;
        Value = value;
        Tooltip = tooltip;
    }
    public object Value { get; }
    public string Styles { get; }

    public string Tooltip { get; }

    public static SpecialField Center(object value) => new(value, "text-align: center;");

    public static SpecialField Right(object value) => new(value, "text-align: right; padding - right: 5px;");

    public static SpecialField Left(object value) => new(value, "text-align: left;");
}