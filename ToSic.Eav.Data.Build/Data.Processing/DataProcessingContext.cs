namespace ToSic.Eav.Data.Processing;

/// <summary>
/// Optional context for <see cref="IDataProcessor"/> actions when the verb alone
/// is not enough to identify the trigger source.
/// </summary>
public record DataProcessingContext
{
    public string? Source { get; init; }
}

public static class DataProcessingContextSources
{
    public const string ContentType = "content-type";
    public const string ContentTypeField = "content-type-field";
}
