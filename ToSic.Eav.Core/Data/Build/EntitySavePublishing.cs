namespace ToSic.Eav.Data.Build;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record EntitySavePublishing
{
    public bool ShouldPublish { get; init; }

    public bool ShouldBranchDrafts { get; init; }

    public override string ToString() => $"ShouldPublish: {ShouldPublish}; ShouldBranchDrafts: {ShouldBranchDrafts}";
}