namespace ToSic.Eav.Data.Publishing.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record PublishingOptions
{
    public required bool ForceDraft { get; init; }
}