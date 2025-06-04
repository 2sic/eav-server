namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record EntitySavePublishing
{
    public bool ShouldPublish { get; init; } = true;

    public bool ShouldBranchDrafts { get; init; } = false;
}