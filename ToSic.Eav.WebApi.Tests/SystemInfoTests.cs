using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSource;
using ToSic.Eav.Run.Startup;
using ToSic.Eav.Services;
using ToSic.Eav.WebApi.Sys.Admin;
using ToSic.Sys.Logging;
using static Xunit.Assert;

namespace ToSic.Eav;

public class SystemInfoTests
{
    [Theory]
    [InlineData(false, 2, 1)]
    [InlineData(true, 0, 0)]
    public void Messages_CountsWarningHistoriesOncePerSegment(bool empty, int expectedOther, int expectedObsolete)
    {
        ImmutableArray<InsightsLogGroupSnapshot> groups = empty
            ? []
            : [Group("warnings-obsolete", 2), Group("warnings-not-implemented", 1), Group("warnings-other", 1), Group("module", 1)];
        var reader = new TestSnapshotReader(new(false, groups));
        var services = new ServiceCollection();
        services.AddEavAll(useMel: true).AddEavAllFallbacks().AddEavWebApiTypedAfterEav();
        services.AddSingleton<IInsightsLogSnapshotReader>(reader);
        using var provider = services.BuildServiceProvider();

        var source = provider.GetRequiredService<IDataSourcesService>().Create(typeof(SystemInfo),
            new DataSourceOptions { AppIdentityOrReader = new AppIdentity(1, 1) });
        var messages = Single(source.GetStream("Messages")!.List);

        Equal(expectedOther, Convert.ToInt32(messages.Get("WarningsOther")));
        Equal(expectedObsolete, Convert.ToInt32(messages.Get("WarningsObsolete")));
        Equal(1, reader.SnapshotCalls);
    }

    private static InsightsLogGroupSnapshot Group(string segment, int eventCount)
    {
        var entry = new InsightsLogEventSnapshot(1, DateTime.UtcNow, "test", InsightsLogLevel.Warning, 0,
            null, null, ImmutableDictionary<string, string?>.Empty, ImmutableDictionary<string, string?>.Empty,
            null, null, null, null, null, null, null, null, segment, null, null, false, false, false,
            null, null, false, false);
        return new(segment, null, [segment], DateTime.UtcNow, ImmutableDictionary<string, string?>.Empty,
            Enumerable.Repeat(entry, eventCount).ToImmutableArray());
    }

    private sealed class TestSnapshotReader(InsightsLogSnapshot snapshot) : IInsightsLogSnapshotReader
    {
        public int SnapshotCalls { get; private set; }
        public InsightsLogSnapshot Snapshot()
        {
            SnapshotCalls++;
            return snapshot;
        }

        public ImmutableArray<InsightsLogGroupSnapshot> ListGroups() => snapshot.Groups;
        public InsightsLogGroupSnapshot? ReadGroup(string groupId) => snapshot.Groups.FirstOrDefault(group => group.Id == groupId);
        public void Pause() => throw new NotSupportedException();
        public void Resume() => throw new NotSupportedException();
        public void FlushSegment(string segment) => throw new NotSupportedException();
        public void Flush() => throw new NotSupportedException();
    }
}
