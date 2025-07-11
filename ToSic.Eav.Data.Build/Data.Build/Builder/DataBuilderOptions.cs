namespace ToSic.Eav.Data.Build;

public record DataBuilderOptions
{
    public bool AllowUnknownValueTypes { get; init; }

    public LogSettings LogSettings { get; init; } = new();
}