using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
using ToSic.Eav.Serialization.Sys.Json;
using ToSic.Eav.Sys;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Utils.Compression;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Work.Tests.RecycleBin;

[Startup(typeof(StartupTestWork))]
public class WorkEntityRecycleBinTests(
    Generator<DbStorage, StorageOptions> dbDataGenerator,
    GenWorkDb<WorkEntityRecycleBin> workEntityRecycleBin,
    ImportService importService,
    DataBuilder dataBuilder)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    private sealed class Specs : IAppIdentity
    {
        public int ZoneId => 4;
        public int AppId => 3012;
    }

    private static readonly Specs TestSpecs = new();

    [Fact]
    public void Get_IncludesDeletedEntity_WithDeletedMeta()
    {
        // Arrange
        var dc = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        // Create a new entity (so we don't mutate scenario-fixed entities).
        var appReader = dc.Loader.AppReaderRaw(TestSpecs.AppId, new());
        var templateEntity = appReader.List.First();
        var now = DateTime.UtcNow;

        var newGuid = Guid.NewGuid();
        var newChild = dataBuilder.Entity.Create(
            appId: TestSpecs.AppId,
            contentType: templateEntity.Type,
            attributes: dataBuilder.Attribute.Create(templateEntity.Type, preparedValues: null),
            entityId: 0,
            repositoryId: 0,
            guid: newGuid,
            titleField: templateEntity.Type.TitleFieldName,
            created: now,
            modified: now,
            owner: "test",
            version: 1,
            isPublished: true
        );

        importService
            .Init(zoneId: TestSpecs.ZoneId, appId: TestSpecs.AppId, skipExistingAttributes: false, preserveUntouchedAttributes: false)
            .ImportIntoDb([], [newChild]);

        var rows = dc.SqlDb.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == TestSpecs.AppId && e.EntityGuid == newGuid && e.TransDeletedId == null)
            .Select(e => new
            {
                e.EntityId,
                e.IsPublished,
                e.PublishedEntityId,
            })
            .ToList();

        var published = rows.SingleOrDefault(r => r.IsPublished);
        True(published != null,
            $"Expected a published entity row for guid {newGuid}, but none found. Rows: {string.Join(", ", rows.Select(r => $"{r.EntityId}/pub:{r.IsPublished}/pubId:{r.PublishedEntityId}"))}");

        var childId = published!.EntityId;

        // Simulate a delete by creating a transaction row and setting TransDeletedId.
        // (This test focuses on the recycle-bin query; delete mechanics are tested elsewhere.)
        var dc2 = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var deleteTrans = new TsDynDataTransaction
        {
            Timestamp = DateTime.UtcNow,
            User = "test-user",
        };
        dc2.SqlDb.TsDynDataTransactions.Add(deleteTrans);
        dc2.SqlDb.SaveChanges();

        var entToDelete = dc2.SqlDb.TsDynDataEntities
            .IgnoreQueryFilters()
            .Single(e => e.EntityId == childId);
        entToDelete.TransDeletedId = deleteTrans.TransactionId;
        dc2.SqlDb.SaveChanges();

        var dbEnt = dc2.SqlDb.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleOrDefault(e => e.EntityId == childId);
        NotNull(dbEnt);
        True(dbEnt!.IsPublished);
        NotNull(dbEnt!.TransDeletedId);

        var appReader2 = dc2.Loader.AppReaderRaw(TestSpecs.AppId, new());
        var recycler = workEntityRecycleBin.New(appReader2);
        var items = recycler.Get();

        var found = items.FirstOrDefault(i => i.Id == childId);
        var sample = string.Join(", ", items.Take(10).Select(i => $"{i.Id}:{i.Guid}"));
        True(found != null,
            $"RecycleBin did not contain expected entityId {childId} (guid {newGuid}). Count:{items.Count}. Sample:{sample}");

        Equal(childId, found!.Id);
        Equal(TestSpecs.AppId, found.AppId);
        NotEqual(0, found.TransactionId);
        True(found.Deleted > DateTime.MinValue);
        Equal("test-user", found.DeletedBy);
        NotEmpty(found.TypeNameId);
        NotEmpty(found.TypeName);

        Equal(dbEnt.TransDeletedId!.Value, found.TransactionId);
    }

    [Fact]
    public void Get_IncludesHistoryOnlyEntity_WhenEntityRowIsMissing_AndJsonIsCompressed()
    {
        // Arrange
        var dc = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var appReader = dc.Loader.AppReaderRaw(TestSpecs.AppId, new());
        var templateEntity = appReader.List.First();

        var sourceId = int.MaxValue;
        var entityGuid = Guid.NewGuid();
        var parentRef = DbVersioning.ParentRefForApp(TestSpecs.AppId);

        var txTimestamp = DateTime.UtcNow;
        var tx = new TsDynDataTransaction
        {
            Timestamp = txTimestamp,
            User = "history-only-test",
        };
        dc.SqlDb.TsDynDataTransactions.Add(tx);
        dc.SqlDb.SaveChanges();

        var json = JsonSerializer.Serialize(
            new JsonFormat
            {
                Entity = new JsonEntity
                {
                    Id = sourceId,
                    Guid = entityGuid,
                    Version = 1,
                    Type = new JsonType { Id = templateEntity.Type.NameId, Name = templateEntity.Type.Name },
                    Attributes = new JsonAttributes(),
                }
            },
            JsonOptions.UnsafeJsonWithoutEncodingHtml);

        var cJson = new Compressor().CompressOrNullIfDisabled(json);
        NotNull(cJson);

        var history = new TsDynDataHistory
        {
            SourceTable = "ToSIC_EAV_Entities",
            SourceId = sourceId,
            SourceGuid = entityGuid,
            Operation = EavConstants.HistoryEntityJson,
            Timestamp = txTimestamp,
            TransactionId = tx.TransactionId,
            ParentRef = parentRef,
            Json = null,
            CJson = cJson,
            Transaction = tx,
        };

        dc.SqlDb.TsDynDataHistories.Add(history);
        dc.SqlDb.SaveChanges();

        // Act
        var recycler = workEntityRecycleBin.New(appId: TestSpecs.AppId);
        var items = recycler.Get();

        // Assert
        var found = items.FirstOrDefault(i => i.Id == sourceId);
        NotNull(found);

        Equal(sourceId, found!.Id);
        Equal(entityGuid, found.Guid);
        Equal(TestSpecs.AppId, found.AppId);

        Equal(tx.TransactionId, found.TransactionId);
        True((found.Deleted - txTimestamp).Duration() < TimeSpan.FromSeconds(2),
            $"Expected Deleted near {txTimestamp:o} but was {found.Deleted:o}");
        Equal("history-only-test", found.DeletedBy);

        Equal(templateEntity.Type.NameId, found.TypeNameId);
        Equal(templateEntity.Type.Name, found.TypeName);
        Equal(parentRef, found.ParentRef);
    }
}
