namespace ToSic.Eav.Data.Build;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntitySavePublishing(bool shouldPublish, bool shouldBranchDrafts)
{
    public bool ShouldPublish { get; } = shouldPublish;

    public bool ShouldBranchDrafts { get; } = shouldBranchDrafts;

    public override string ToString() => $"ShouldPublish: {ShouldPublish}; ShouldBranchDrafts: {ShouldBranchDrafts}";
}