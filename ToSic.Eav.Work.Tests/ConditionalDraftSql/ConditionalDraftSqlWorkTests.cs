using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Sys.Dimensions;
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
    IAppReaderFactory readers, AppsCacheSwitch cacheSwitch, ConditionalDraftSqlCache cache, ConditionalDraftSqlFeatures features)
{
    private readonly ConditionalDraftSqlFixture _sql = new(storage, loaders, environment, saver, assembler, fixture, boot, stores);

    [C3SqlTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Save_PublishesCacheOnlyAfterIndependentSqlReadSeesCommittedDraft(bool optimized)
    {
        var expected = Prepare(optimized);
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

        var modeChecks = features.SaveModeChecks;
        writers.New(context).Save(entity, options);

        Assert.True(features.SaveModeChecks > modeChecks);
        Assert.Equal(1, cache.Updates);
        Assert.Equal(0, cache.Purges);
        Assert.Equal(title, DraftTitle(context.AppReader, expected));
        Assert.Equal(title, DraftTitle(readers.Get(expected.AppId), expected));
        Assert.Equal(2, context.AppReader.List.Count(e => e.EntityGuid == expected.EntityGuid));
        Assert.Equal(publishedTitle, context.AppReader.List.Single(e => e.EntityGuid == expected.EntityGuid && e.IsPublished).GetBestTitle());
    }

    [C3SqlTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Save_DoesNotPublishCacheWhenSqlRejectsTheDraft(bool lateHistoryFailure, bool optimized)
    {
        var expected = Prepare(optimized);
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

    [C3SqlTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Save_RestoresNormalCacheInvalidationAfterSuppressedActionThrows(bool optimized)
    {
        var expected = Prepare(optimized);
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

    private ExistingDraftSaveCondition Prepare(bool optimized)
    {
        features.Optimized = optimized;
        return _sql.Prepare();
    }

    [C3SqlTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Save_PreservesUnselectedAttributesAndOtherTitleLanguage(bool optimized)
    {
        var expected = Prepare(optimized);
        _sql.EnsureLanguage("hr-hr", "Croatian (C3 SQL fixture)");
        var seedContext = PrepareWork(expected);
        var english = "C3 English seed " + Guid.NewGuid().ToString("N");
        var croatian = "C3 hrvatski " + Guid.NewGuid().ToString("N");
        var (seed, seedOptions) = _sql.PrepareTitle(seedContext.DbStorage, expected, english);
        IEntity extra = assembler.CreateEntityTac(appId: expected.AppId, entityId: 0, contentType: seed.Type,
            values: new() { { "Amount", 123.45m }, { "Enabled", true },
                { "When", new DateTime(2026, 1, 15, 12, 34, 56, DateTimeKind.Utc) }, { "Optional", "C3 preserve optional" } });
        extra = assembler.Entity.CreateFrom(extra, isPublished: false);
        seed = _sql.MergePartial(seed, extra, seedOptions);
        var title = assembler.Attribute.Create("Title", ValueTypes.String,
        [
            assembler.Value.String(english, [new Language("en-us", false)]),
            assembler.Value.String(croatian, [new Language("hr-hr", false)]),
        ]);
        var attributes = assembler.AttributeList.Replace(seed.Attributes, [title]);
        seed = assembler.Entity.CreateFrom(seed, attributes: assembler.AttributeList.Finalize(attributes));
        writers.New(seedContext).Save(seed, seedOptions);

        expected = _sql.Prepare();
        var context = PrepareWork(expected);
        var original = RawDraft(expected);
        Assert.Equal(english, original.Attributes["Title"]["en-us"]);
        Assert.Equal(croatian, original.Attributes["Title"]["hr-hr"]);
        Assert.Equal("C3 preserve optional", original.Attributes["Optional"]["en-us"]);
        var originalSnapshot = Snapshot(original, selectedEnglishTitle: false);
        var preserved = Snapshot(original, selectedEnglishTitle: true);
        var changedTitle = "C3 changed English " + Guid.NewGuid().ToString("N");
        var (changed, options) = _sql.PrepareTitle(context.DbStorage, expected, changedTitle);

        writers.New(context).Save(changed, options);

        var after = RawDraft(expected);
        Assert.Equal(original.Attributes.Keys.OrderBy(k => k), after.Attributes.Keys.OrderBy(k => k));
        Assert.Equal(preserved, Snapshot(after, selectedEnglishTitle: true));
        Assert.Equal(originalSnapshot, Snapshot(original, selectedEnglishTitle: false));
        Assert.Equal(changedTitle, after.Attributes["Title"]["en-us"]);
        Assert.Equal(croatian, after.Attributes["Title"]["hr-hr"]);
        Assert.Equal(changedTitle, context.AppReader.List.Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished).Attributes["Title"]["en-us"]);
        Assert.Equal(croatian, context.AppReader.List.Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished).Attributes["Title"]["hr-hr"]);
    }

    private IEntity RawDraft(ExistingDraftSaveCondition expected)
        => loaders.New().UseExistingDb(storage.SqlDb).AppStateReaderRawTac(expected.AppId).List
            .Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished);

    private static string[] Snapshot(IEntity entity, bool selectedEnglishTitle)
        => entity.Attributes.SelectMany(a => a.Value.Values
                .Where(v => !selectedEnglishTitle || a.Key != "Title" || !v.Languages.Any(l => l.Key == "en-us"))
                .Select(v => JsonSerializer.Serialize(new
                {
                    Field = a.Key,
                    Type = a.Value.Type,
                    Value = v.Serialized,
                    Languages = v.Languages.OrderBy(l => l.Key).Select(l => new { l.Key, l.ReadOnly }).ToArray(),
                })))
            .OrderBy(v => v, StringComparer.Ordinal).ToArray();

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
