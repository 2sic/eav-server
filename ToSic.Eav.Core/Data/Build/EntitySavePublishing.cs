namespace ToSic.Eav.Data.Build;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record EntitySavePublishing
{
    public bool ShouldPublish { get; init; } = true;

    public bool ShouldBranchDrafts { get; init; } = false;

    public override string ToString() => $"ShouldPublish: {ShouldPublish}; ShouldBranchDrafts: {ShouldBranchDrafts}";
}