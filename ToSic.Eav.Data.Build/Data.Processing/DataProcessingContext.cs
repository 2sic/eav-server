namespace ToSic.Eav.Data.Processing;

/// <summary>
/// Optional context for <see cref="IDataProcessor"/> actions when the verb alone
/// is not enough to identify the trigger source.
/// This is the authoritative carrier for schema-triggered processor runs where
/// there is no real entity payload and <c>Data</c> may intentionally be null.
/// </summary>
public record DataProcessingContext
{
    public string? Source { get; init; }

    public int? AppId { get; init; }

    public int? ContentTypeId { get; init; }

    public string? ContentTypeNameId { get; init; }
}

public static class DataProcessingContextSources
{
    public const string ContentType = "content-type";
    public const string ContentTypeField = "content-type-field";
}
