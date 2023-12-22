namespace ToSic.Eav.Data.Build
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class EntitySavePublishing
    {
        public EntitySavePublishing(bool shouldPublish, bool shouldBranchDrafts)
        {
            ShouldPublish = shouldPublish;
            ShouldBranchDrafts = shouldBranchDrafts;
        }

        public bool ShouldPublish { get; }

        public bool ShouldBranchDrafts { get; }

        public override string ToString() => $"ShouldPublish: {ShouldPublish}; ShouldBranchDrafts: {ShouldBranchDrafts}";
    }
}
