using System.Collections.Immutable;

namespace ToSic.Sys.Logging;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IInsightsLogSnapshotReader
{
    InsightsLogSnapshot Snapshot();
    ImmutableArray<InsightsLogGroupSnapshot> ListGroups();
    InsightsLogGroupSnapshot? ReadGroup(string groupId);
    void Pause();
    void Resume();
    void FlushSegment(string segment);
    void Flush();
}
