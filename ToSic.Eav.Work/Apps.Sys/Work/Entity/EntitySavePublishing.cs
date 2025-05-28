namespace ToSic.Eav.Data.Build;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record EntitySavePublishing
{
    public bool ShouldPublish { get; init; } = true;

    public bool ShouldBranchDrafts { get; init; } = false;
}