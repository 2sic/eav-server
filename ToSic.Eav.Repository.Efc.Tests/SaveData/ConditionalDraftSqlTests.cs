using ToSic.Eav.Data.Build;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ToSic.Eav.Data;
using ToSic.Eav.Repositories;
using ToSic.Sys.Boot;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Testing.Scenarios;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Repository.Efc.Tests.SaveData;

public record ScenarioConditionalDraftSql : ScenarioBasic
{
    public override string ConStr => System.Environment.GetEnvironmentVariable("SXC_C3_SQL_CONNECTION")
        ?? throw new InvalidOperationException("Explicit owned SQL fixture opt-in is required.");
    public override string GlobalFolder => Path.Combine(System.Environment.GetEnvironmentVariable("SXC_C3_WORKSPACE")!, "2sxc", "Src", "Data");
    public override string GlobalDataCustomFolder => Path.Combine(System.Environment.GetEnvironmentVariable("SXC_C3_WORKSPACE")!, "nonexistent-c3-custom-data");
    public override string AppsShared => Path.Combine(System.Environment.GetEnvironmentVariable("SXC_C3_WORKSPACE")!, "eav-server", "test-data", "mock-site-structure", "shared");
}

public sealed class C3SqlFactAttribute : FactAttribute
{
    public C3SqlFactAttribute()
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SXC_C3_SQL_CONNECTION")))
            Skip = "Requires explicitly provisioned, marked C3 SQL fixture; never use the shared eav-testing database.";
    }
}

public sealed class C3SqlTheoryAttribute : TheoryAttribute
{
    public C3SqlTheoryAttribute()
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SXC_C3_SQL_CONNECTION")))
            Skip = "Requires explicitly provisioned, marked C3 SQL fixture; never use the shared eav-testing database.";
    }
}

[Startup(typeof(StartupTestsApps))]
public class ConditionalDraftSqlTests(DbStorage storage, Generator<EfcAppLoaderService> loaders, IImportExportEnvironment environment,
    EntitySaver saver, DataAssembler assembler, FixtureStartupWithDb fixture, BootCoordinator boot, Generator<DbStorage> stores)
{
    [C3SqlFact]
    public void Save_UpdatesExistingDraft_AndPreservesPublishedRow()
    {
        var expected = Prepare();
        var before = ReadPair(storage, expected);
        var title = "C3 conditional draft " + Guid.NewGuid().ToString("N");
        SaveTitle(storage, expected, title);
        var after = ReadPair(storage, expected);
        AssertPublishedUnchanged(before, after, expected);
        var draftAfter = after.Single(e => e.EntityId == expected.DraftId);
        Assert.Equal(expected.DraftVersion + 1, draftAfter.Version);
        Assert.False(draftAfter.IsPublished);
        Assert.Equal(expected.EntityId, draftAfter.PublishedEntityId);
        Assert.Equal(title, ReadTitle(storage, expected));
    }

    [C3SqlFact]
    public void Save_RejectsStaleRevisionAfterOrdinaryNativeWriter()
    {
        var expected = Prepare();
        var writer = stores.New();
        writer.Setup(new(ZoneId, expected.AppId, null, new()));
        using var writerDb = writer.SqlDb;
        Assert.NotSame(storage.SqlDb, writerDb);
        var title = "C3 ordinary writer " + Guid.NewGuid().ToString("N");
        SaveTitle(writer, expected, title, conditional: false);
        var afterOrdinaryWrite = ReadPair(storage, expected);
        Assert.Equal(expected.DraftVersion + 1, afterOrdinaryWrite.Single(e => e.EntityId == expected.DraftId).Version);
        Assert.Equal(title, ReadTitle(writer, expected));

        Assert.Throws<System.Data.DBConcurrencyException>(() => SaveTitle(storage, expected, "must not persist"));

        AssertPairUnchanged(afterOrdinaryWrite, ReadPair(storage, expected));
        Assert.Equal(title, ReadTitle(storage, expected));
    }

    [C3SqlTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Save_RollsBackWhenSqlRejectsDraftUpdate(bool conditional)
    {
        var expected = Prepare();
        var before = ReadPair(storage, expected);
        var transactionCount = storage.SqlDb.TsDynDataTransactions.Count();
        var historyCount = storage.SqlDb.TsDynDataHistories.Count();
        var autoDetectChanges = storage.SqlDb.ChangeTracker.AutoDetectChangesEnabled;
        var constraint = "C3Reject_" + Guid.NewGuid().ToString("N");
        var title = "C3 rejected " + Guid.NewGuid().ToString("N");
        Execute(storage, "ALTER TABLE " + EntityTable(storage) + " ADD CONSTRAINT [" + constraint + "] CHECK (EntityId<>" +
            expected.DraftId + " OR Version<>" + (expected.DraftVersion + 1) + ")");
        try
        {
            var error = Assert.ThrowsAny<Exception>(() => SaveTitle(storage, expected, title, conditional));
            Assert.Contains(constraint, error.ToString());
            AssertPairUnchanged(before, ReadPair(storage, expected));
            Assert.False(storage.IsWithinSave);
            Assert.Equal(autoDetectChanges, storage.SqlDb.ChangeTracker.AutoDetectChangesEnabled);
            Assert.Null(storage.SqlDb.Database.CurrentTransaction);
            Assert.Equal(transactionCount, storage.SqlDb.TsDynDataTransactions.Count());
            Assert.Equal(historyCount, storage.SqlDb.TsDynDataHistories.Count());
        }
        finally
        {
            Execute(storage, "ALTER TABLE " + EntityTable(storage) + " DROP CONSTRAINT [" + constraint + "]");
        }
    }

    [C3SqlFact]
    public void Save_RestoresInternalStateWhenPreparationThrows()
    {
        var expected = Prepare();
        var before = ReadPair(storage, expected);
        var autoDetectChanges = storage.SqlDb.ChangeTracker.AutoDetectChangesEnabled;
        var failure = new InvalidOperationException("C3 deliberate preparation failure");

        var actual = Assert.Throws<InvalidOperationException>(() => storage.DoAndSaveWithoutChangeDetection(() => throw failure));

        Assert.Same(failure, actual);
        Assert.False(storage.IsWithinSave);
        Assert.Equal(autoDetectChanges, storage.SqlDb.ChangeTracker.AutoDetectChangesEnabled);
        AssertPairUnchanged(before, ReadPair(storage, expected));
    }

    [C3SqlFact]
    public async Task Save_BlocksOrdinarySqlWriterBetweenRevisionCheckAndUpdate()
    {
        var expected = Prepare();
        var before = ReadPair(storage, expected);
        var title = "C3 serialized save " + Guid.NewGuid().ToString("N");
        var save = PrepareTitleSave(storage, expected, title);
        var gate = stores.New();
        gate.Setup(new(ZoneId, expected.AppId, null, new()));
        using var gateDb = gate.SqlDb;
        gateDb.Database.OpenConnection();
        var writer = stores.New();
        writer.Setup(new(ZoneId, expected.AppId, null, new()));
        using var writerDb = writer.SqlDb;
        writerDb.Database.OpenConnection();
        Assert.NotSame(storage.SqlDb, writerDb);
        Assert.NotSame(storage.SqlDb, gateDb);
        var nativeSession = Scalar(storage, "SELECT @@SPID");
        var gateSession = Scalar(gate, "SELECT @@SPID");
        var writerSession = Scalar(writer, "SELECT @@SPID");

        // Pause at the first native audit INSERT, after the revision guard but before the entity UPDATE.
        // The competing SQL writer has no MCP lock or knowledge of ExistingDraftSaveCondition.
        using var hold = gateDb.Database.BeginTransaction();
        Execute(gate, "SELECT TOP(1) TransactionId FROM [dbo].[TsDynDataTransaction] WITH(TABLOCKX,HOLDLOCK)");
        Task? native = null;
        Task? ordinary = null;
        try
        {
            native = Task.Run(save);
            await DemandBlocked(gate, nativeSession, gateSession, native, "INSERT INTO [TsDynDataTransaction]");
            ordinary = Task.Run(() => Execute(writer, "UPDATE " + EntityTable(writer) + " SET Version=Version+1 WHERE EntityId=" + expected.DraftId));
            await DemandBlocked(gate, writerSession, nativeSession, ordinary, "UPDATE");
            hold.Commit();
            await Task.WhenAll(native, ordinary);

            var after = ReadPair(storage, expected);
            AssertPublishedUnchanged(before, after, expected);
            Assert.Equal(expected.DraftVersion + 2, after.Single(e => e.EntityId == expected.DraftId).Version);
            Assert.Equal(title, ReadTitle(storage, expected));
        }
        finally
        {
            // Release the barrier before draining workers, including when an assertion fails.
            hold.Dispose();
            if (native != null)
                try { await native; } catch { /* Keep the original test failure. */ }
            if (ordinary != null)
                try { await ordinary; } catch { /* Keep the original test failure. */ }
        }
    }

    private static async Task DemandBlocked(DbStorage observer, int session, int blocker, Task worker, string statement)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            using var command = observer.SqlDb.Database.GetDbConnection().CreateCommand();
            command.Transaction = observer.SqlDb.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = "SELECT t.text FROM sys.dm_exec_requests r CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t " +
                "WHERE r.session_id=" + session + " AND r.blocking_session_id=" + blocker + " AND r.wait_type LIKE N'LCK%';";
            if (command.ExecuteScalar() is string sql && sql.Contains(statement))
                return;
            if (worker.IsCompleted)
            {
                await worker;
                Assert.Fail("Writer completed before the expected SQL row-lock barrier was observed.");
            }
            await Task.Delay(40);
        }
        Assert.Fail("Expected SQL blocking relationship was not observed within ten seconds.");
    }

    private ExistingDraftSaveCondition Prepare()
    {
        fixture.SetupFixtureConfiguration(new ScenarioConditionalDraftSql());
        boot.StartUp();
        var appId = int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_APP")!);
        var zoneId = ZoneId;
        var entityId = int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_ENTITY")!);
        var guid = Guid.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_GUID")!);
        storage.Setup(new(zoneId, appId, null, new()));
        var database = storage.SqlDb.Database.GetDbConnection().Database;
        Assert.StartsWith("SxcMcpC3_", database);
        storage.SqlDb.Database.OpenConnection();
        using (var marker = storage.SqlDb.Database.GetDbConnection().CreateCommand())
        {
            marker.CommandText = "SELECT CONVERT(nvarchar(64),value) FROM sys.extended_properties WHERE class=0 AND name=N'2sxc-mcp-c3-owner'";
            Assert.Equal(System.Environment.GetEnvironmentVariable("SXC_C3_NONCE"), marker.ExecuteScalar());
        }
        // The generic test host does not run DNN's on-demand language synchronization.
        // Provision the QA language only in this marked copy, through the native dimension service.
        var dimensions = stores.New();
        dimensions.Setup(new(zoneId, appId, null, new()));
        using (dimensions.SqlDb)
            if (!dimensions.SqlDb.TsDynDataDimensions.Any(d => d.ZoneId == zoneId && d.EnvironmentKey == "en-us"))
                dimensions.Dimensions.AddOrUpdateZoneDimension("en-us", "English (C3 SQL fixture)", true);
        var rows = storage.SqlDb.TsDynDataEntities.AsNoTracking().Where(e => e.AppId == appId && e.EntityGuid == guid && e.TransDeletedId == null).ToArray();
        var published = rows.Single(e => e.EntityId == entityId && e.IsPublished);
        var draft = rows.Single(e => e.PublishedEntityId == entityId && !e.IsPublished);
        return new(appId, entityId, guid, published.Version, draft.EntityId, draft.Version);
    }

    private void SaveTitle(DbStorage target, ExistingDraftSaveCondition expected, string title, bool conditional = true)
        => PrepareTitleSave(target, expected, title, conditional)();

    private Action PrepareTitleSave(DbStorage target, ExistingDraftSaveCondition expected, string title, bool conditional = true)
    {
        var app = loaders.New().UseExistingDb(target.SqlDb).AppStateReaderRawTac(expected.AppId);
        var original = app.List.Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished);
        var options = environment.SaveOptions(ZoneId) with
        {
            Languages = target.SqlDb.TsDynDataDimensions.AsNoTracking().Include(d => d.ParentNavigation)
                .Where(d => d.ZoneId == ZoneId && d.Parent != null).ToList().Select(d => d.AsDimensionDefinition).ToList(),
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
            DraftShouldBranch = !conditional,
            ExistingDraftCondition = conditional ? expected : null,
        };
        IEntity partial = assembler.CreateEntityTac(appId: expected.AppId, entityId: 0, contentType: original.Type,
            values: new() { { "Title", title } });
        partial = assembler.Entity.CreateFrom(partial, isPublished: false);
        var merged = saver.TestCreateMergedForSavingTac(original, partial, options);
        Assert.False(merged.IsPublished);

        return () => target.Save(options.AddToAll([merged]));
    }

    private TsDynDataEntity[] ReadPair(DbStorage target, ExistingDraftSaveCondition expected)
        => target.SqlDb.TsDynDataEntities.AsNoTracking().Where(e => e.AppId == expected.AppId && e.EntityGuid == expected.EntityGuid && e.TransDeletedId == null).ToArray();

    private static int ZoneId => int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_ZONE")!);

    private string? ReadTitle(DbStorage target, ExistingDraftSaveCondition expected)
        => loaders.New().UseExistingDb(target.SqlDb).AppStateReaderRawTac(expected.AppId).List
            .Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished).GetBestTitle();

    private static void AssertPublishedUnchanged(TsDynDataEntity[] before, TsDynDataEntity[] after, ExistingDraftSaveCondition expected)
        => AssertPairUnchanged(before.Where(e => e.EntityId == expected.EntityId).ToArray(), after.Where(e => e.EntityId == expected.EntityId).ToArray());

    private static void AssertPairUnchanged(TsDynDataEntity[] before, TsDynDataEntity[] after)
    {
        Assert.Equal(before.Length, after.Length);
        foreach (var row in before)
        {
            var actual = after.Single(e => e.EntityId == row.EntityId);
            Assert.Equal(row.Version, actual.Version);
            Assert.Equal(row.Json, actual.Json);
            Assert.Equal(row.TransModifiedId, actual.TransModifiedId);
            Assert.Equal(row.IsPublished, actual.IsPublished);
            Assert.Equal(row.PublishedEntityId, actual.PublishedEntityId);
            Assert.Equal(row.EntityGuid, actual.EntityGuid);
            Assert.Equal(row.AppId, actual.AppId);
            Assert.Equal(row.ContentTypeId, actual.ContentTypeId);
            Assert.Equal(row.ContentType, actual.ContentType);
            Assert.Equal(row.Owner, actual.Owner);
            Assert.Equal(row.TransCreatedId, actual.TransCreatedId);
            Assert.Equal(row.TransDeletedId, actual.TransDeletedId);
        }
    }

    private static void Execute(DbStorage target, string sql)
    {
        using var command = target.SqlDb.Database.GetDbConnection().CreateCommand();
        command.Transaction = target.SqlDb.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static int Scalar(DbStorage target, string sql)
    {
        using var command = target.SqlDb.Database.GetDbConnection().CreateCommand();
        command.Transaction = target.SqlDb.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = sql;
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static string EntityTable(DbStorage target)
    {
        var model = target.SqlDb.Model.FindEntityType(typeof(TsDynDataEntity))!;
        string Quote(string value) => "[" + value.Replace("]", "]]") + "]";
#if NETFRAMEWORK
        return Quote(model.Relational().Schema ?? "dbo") + "." + Quote(model.Relational().TableName);
#else
        return Quote(model.GetSchema() ?? "dbo") + "." + Quote(model.GetTableName()!);
#endif
    }
}
