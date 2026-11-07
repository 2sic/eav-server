using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Repository.Efc.Tests.SaveData;
using ToSic.Sys.Boot;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Work.Tests.ConditionalDraftSql;

[Startup(typeof(StartupConditionalDraftSqlWork))]
public class ConditionalDraftSqlWorkTests(DbStorage storage, Generator<EfcAppLoaderService> loaders,
    IImportExportEnvironment environment, EntitySaver saver, DataAssembler assembler,
    FixtureStartupWithDb fixture, BootCoordinator boot, Generator<DbStorage> stores,
    AppWorkContextService contexts, Generator<WorkEntitySave, IAppWorkContext> writers,
    IAppReaderFactory readers, AppsCacheSwitch cacheSwitch, ConditionalDraftSqlCache cache)
{
    private readonly ConditionalDraftSqlFixture _sql = new(storage, loaders, environment, saver, assembler, fixture, boot, stores);

    [C3SqlFact]
    public void Save_PublishesCacheOnlyAfterIndependentSqlReadSeesCommittedDraft()
    {
        var expected = _sql.Prepare();
        var context = PrepareWork(expected);
        var oldTitle = DraftTitle(context.AppReader, expected);
        var publishedTitle = context.AppReader.List.Single(e => e.EntityGuid == expected.EntityGuid && e.IsPublished).GetBestTitle();
        var title = "C3 committed cache " + Guid.NewGuid().ToString("N");
        var (entity, options) = _sql.PrepareTitle(context.DbStorage, expected, title);
        cache.BeforeUpdate = () =>
        {
            Assert.Null(context.DbStorage.SqlDb.Database.CurrentTransaction);
            Assert.NotSame(storage.SqlDb, context.DbStorage.SqlDb);
            var committed = storage.SqlDb.TsDynDataEntities.AsNoTracking().Single(e => e.EntityId == expected.DraftId);
            Assert.Equal(expected.DraftVersion + 1, committed.Version);
            Assert.Equal(title, RawTitle(expected));
            Assert.Equal(oldTitle, DraftTitle(context.AppReader, expected));
        };

        writers.New(context).Save(entity, options);

        Assert.Equal(1, cache.Updates);
        Assert.Equal(0, cache.Purges);
        Assert.Equal(title, DraftTitle(context.AppReader, expected));
        Assert.Equal(title, DraftTitle(readers.Get(expected.AppId), expected));
        Assert.Equal(2, context.AppReader.List.Count(e => e.EntityGuid == expected.EntityGuid));
        Assert.Equal(publishedTitle, context.AppReader.List.Single(e => e.EntityGuid == expected.EntityGuid && e.IsPublished).GetBestTitle());
    }

    [C3SqlTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Save_DoesNotPublishCacheWhenSqlRejectsTheDraft(bool lateHistoryFailure)
    {
        var expected = _sql.Prepare();
        var context = PrepareWork(expected);
        var oldTitle = DraftTitle(context.AppReader, expected);
        var before = storage.SqlDb.TsDynDataEntities.AsNoTracking().Single(e => e.EntityId == expected.DraftId);
        var transactionCount = storage.SqlDb.TsDynDataTransactions.Count();
        var historyCount = storage.SqlDb.TsDynDataHistories.Count();
        var title = "C3 rejected cache " + Guid.NewGuid().ToString("N");
        var (entity, options) = _sql.PrepareTitle(context.DbStorage, expected, title);
        var constraint = "C3Reject_" + Guid.NewGuid().ToString("N");
        var table = lateHistoryFailure ? "[dbo].[TsDynDataHistory]" : "[dbo].[TsDynDataEntity]";
        var predicate = lateHistoryFailure
            ? "SourceId<>" + expected.DraftId + " OR TransactionId<=" + storage.SqlDb.TsDynDataTransactions.Max(t => t.TransactionId)
            : "EntityId<>" + expected.DraftId + " OR Version<>" + (expected.DraftVersion + 1);
        Execute("ALTER TABLE " + table + " ADD CONSTRAINT [" + constraint + "] CHECK (" + predicate + ")");
        try
        {
            var error = Assert.ThrowsAny<Exception>(() => writers.New(context).Save(entity, options));
            Assert.Contains(constraint, error.ToString());
            Assert.Equal(0, cache.Updates);
            Assert.Equal(0, cache.Purges);
            Assert.True(cache.Has(context.AppReader));
            Assert.Equal(oldTitle, DraftTitle(context.AppReader, expected));
            Assert.Equal(oldTitle, RawTitle(expected));
            var after = storage.SqlDb.TsDynDataEntities.AsNoTracking().Single(e => e.EntityId == expected.DraftId);
            Assert.Equal(before.Version, after.Version);
            Assert.Equal(before.Json, after.Json);
            Assert.Equal(transactionCount, storage.SqlDb.TsDynDataTransactions.Count());
            Assert.Equal(historyCount, storage.SqlDb.TsDynDataHistories.Count());
            Assert.False(context.DbStorage.IsWithinSave);
        }
        finally
        {
            Execute("ALTER TABLE " + table + " DROP CONSTRAINT [" + constraint + "]");
        }
    }

    [C3SqlFact]
    public void Save_RestoresNormalCacheInvalidationAfterSuppressedActionThrows()
    {
        var expected = _sql.Prepare();
        var context = PrepareWork(expected);
        var failure = new InvalidOperationException("C3 deliberate suppressed action failure");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            context.DbStorage.DoButSkipAppCachePurge(() => throw failure)));
        var (entity, options) = _sql.PrepareTitle(context.DbStorage, expected, "C3 restored purge " + Guid.NewGuid().ToString("N"));

        context.DbStorage.Save(options.AddToAll([entity]));

        Assert.True(cache.Purges > 0);
        Assert.False(cache.Has(context.AppReader));
        Assert.Equal(0, cache.Updates);
    }

    private IAppWorkContext PrepareWork(ExistingDraftSaveCondition expected)
    {
        Assert.Same(cache, cacheSwitch.Value);
        cacheSwitch.Purge(new AppIdentity(ConditionalDraftSqlFixture.ZoneId, expected.AppId));
        var reader = readers.Get(expected.AppId);
        _ = reader.List.Count;
        var context = contexts.ContextNew(reader);
        cache.Reset();
        return context;
    }

    private static string? DraftTitle(IAppReader reader, ExistingDraftSaveCondition expected)
        => reader.List.Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished).GetBestTitle();

    private string? RawTitle(ExistingDraftSaveCondition expected)
        => loaders.New().UseExistingDb(storage.SqlDb).AppStateReaderRawTac(expected.AppId).List
            .Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished).GetBestTitle();

    private void Execute(string sql)
    {
        using var command = storage.SqlDb.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
