using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Serialization.Sys.Json;
using ToSic.Eav.Sys;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Utils.Compression;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Work.Tests.Recycle;

[Startup(typeof(StartupTestWork))]
public class WorkEntityRecycleTests(
    Generator<DbStorage, StorageOptions> dbDataGenerator,
    GenWorkDb<WorkEntityRecycle> workEntityRecycle,
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
    public void Recycle_RestoresDeletedEntity_AndRelationships_ForTransaction()
    {
        // Arrange
        var dc = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        // Pick an existing relationship to get a valid attribute and a valid child.
        var templateRel = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Where(r => r.ChildEntityId != null)
            .Select(r => new
            {
                r.ParentEntityId,
                r.AttributeId,
                ChildEntityId = r.ChildEntityId!.Value,
                ChildGuid = r.ChildEntity!.EntityGuid,
            })
            .FirstOrDefault();

        NotNull(templateRel);

        // Create a new entity (so we don't mutate scenario-fixed entities).
        var appReader = dc.Loader.AppReaderRaw(TestSpecs.AppId, new());
        var templateEntity = appReader.List.First();
        var now = DateTime.UtcNow;

        var newGuid = Guid.NewGuid();
        var newEntity = dataBuilder.Entity.Create(
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
            .ImportIntoDb([], [newEntity]);

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

        var newEntityId = published!.EntityId;

        // Create relationship where the new entity is the child (inbound relationship).
        var inboundSortOrder = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => r.ParentEntityId == templateRel!.ParentEntityId && r.AttributeId == templateRel.AttributeId)
            .Select(r => (int?)r.SortOrder)
            .Max() ?? 0;
        inboundSortOrder++;

        dc.SqlDb.TsDynDataRelationships.Add(new()
        {
            AttributeId = templateRel.AttributeId,
            ParentEntityId = templateRel.ParentEntityId,
            ChildEntityId = newEntityId,
            SortOrder = inboundSortOrder,
        });

        // Create relationship where the new entity is the parent (outbound relationship).
        var outboundSortOrder = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => r.ParentEntityId == newEntityId && r.AttributeId == templateRel.AttributeId)
            .Select(r => (int?)r.SortOrder)
            .Max() ?? 0;
        outboundSortOrder++;

        dc.SqlDb.TsDynDataRelationships.Add(new()
        {
            AttributeId = templateRel.AttributeId,
            ParentEntityId = newEntityId,
            ChildEntityId = templateRel.ChildEntityId,
            SortOrder = outboundSortOrder,
        });

        dc.SqlDb.SaveChanges();

        // Simulate a delete (similar to how recycle-bin tests avoid WorkEntityDelete).
        // - mark entity deleted using a transaction
        // - null out relationship ChildEntityId and store GUID in ChildExternalId
        var dc2 = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var deleteTrans = new TsDynDataTransaction
        {
            Timestamp = DateTime.UtcNow,
            User = "recycle-test",
        };
        dc2.SqlDb.TsDynDataTransactions.Add(deleteTrans);
        dc2.SqlDb.SaveChanges();

        var entToDelete = dc2.SqlDb.TsDynDataEntities
            .IgnoreQueryFilters()
            .Single(e => e.EntityId == newEntityId);
        entToDelete.TransDeletedId = deleteTrans.TransactionId;

        var inboundRelToSoftDelete = dc2.SqlDb.TsDynDataRelationships
            .Single(r => r.ParentEntityId == templateRel.ParentEntityId
                && r.AttributeId == templateRel.AttributeId
                && r.SortOrder == inboundSortOrder);
        inboundRelToSoftDelete.ChildExternalId = newGuid;
        inboundRelToSoftDelete.ChildEntityId = null;

        var outboundRelToSoftDelete = dc2.SqlDb.TsDynDataRelationships
            .Single(r => r.ParentEntityId == newEntityId
                && r.AttributeId == templateRel.AttributeId
                && r.SortOrder == outboundSortOrder);
        outboundRelToSoftDelete.ChildExternalId = templateRel.ChildGuid;
        outboundRelToSoftDelete.ChildEntityId = null;

        dc2.SqlDb.SaveChanges();

        var recycler = workEntityRecycle.New(appId: TestSpecs.AppId);
        recycler.Recycle(transactionId: deleteTrans.TransactionId);

        // Assert
        var dc3 = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var restoredEntity = dc3.SqlDb.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Single(e => e.EntityId == newEntityId);
        Null(restoredEntity.TransDeletedId);

        var inboundRelRestored = dc3.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Single(r => r.ParentEntityId == templateRel.ParentEntityId
                && r.AttributeId == templateRel.AttributeId
                && r.SortOrder == inboundSortOrder);

        Equal(newEntityId, inboundRelRestored.ChildEntityId);
        Null(inboundRelRestored.ChildExternalId);

        var outboundRelRestored = dc3.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Single(r => r.ParentEntityId == newEntityId
                && r.AttributeId == templateRel.AttributeId
                && r.SortOrder == outboundSortOrder);

        Equal(templateRel.ChildEntityId, outboundRelRestored.ChildEntityId);
        Null(outboundRelRestored.ChildExternalId);
    }

    [Fact]
    public void Recycle_RestoresHistoryOnlyEntity_AndInboundRelationships_ForTransaction()
    {
        // Arrange
        var dc = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var appReader = dc.Loader.AppReaderRaw(TestSpecs.AppId, new());
        var templateEntity = appReader.List.First();

        // Pick an existing relationship to get a valid parent + attribute.
        var templateRel = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Where(r => r.ChildEntityId != null)
            .Select(r => new
            {
                r.ParentEntityId,
                r.AttributeId,
            })
            .First();

        var entityGuid = Guid.NewGuid();
        var sourceId = int.MaxValue - 123;
        var parentRef = DbVersioning.ParentRefForApp(TestSpecs.AppId);

        var txTimestamp = DateTime.UtcNow;
        var tx = new TsDynDataTransaction
        {
            Timestamp = txTimestamp,
            User = "history-only-recycle-test",
        };
        dc.SqlDb.TsDynDataTransactions.Add(tx);
        dc.SqlDb.SaveChanges();

        // Create a minimal entity-json package like versioning does.
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

        dc.SqlDb.TsDynDataHistories.Add(new TsDynDataHistory
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
        });

        // Add an inbound relationship which references the deleted entity by external GUID.
        var inboundSortOrder = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => r.ParentEntityId == templateRel.ParentEntityId && r.AttributeId == templateRel.AttributeId)
            .Select(r => (int?)r.SortOrder)
            .Max() ?? 0;
        inboundSortOrder++;

        dc.SqlDb.TsDynDataRelationships.Add(new()
        {
            AttributeId = templateRel.AttributeId,
            ParentEntityId = templateRel.ParentEntityId,
            ChildEntityId = null,
            ChildExternalId = entityGuid,
            SortOrder = inboundSortOrder,
        });

        dc.SqlDb.SaveChanges();

        // Act
        var recycler = workEntityRecycle.New(appId: TestSpecs.AppId);
        recycler.Recycle(transactionId: tx.TransactionId);

        // Assert
        var dc2 = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var restoredEntity = dc2.SqlDb.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Single(e => e.AppId == TestSpecs.AppId && e.EntityGuid == entityGuid);
        Null(restoredEntity.TransDeletedId);

        var inboundRelRestored = dc2.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Single(r => r.ParentEntityId == templateRel.ParentEntityId
                && r.AttributeId == templateRel.AttributeId
                && r.SortOrder == inboundSortOrder);

        Equal(restoredEntity.EntityId, inboundRelRestored.ChildEntityId);
        Null(inboundRelRestored.ChildExternalId);
    }

    [Fact]
    public void Recycle_RestoresHistoryOnlyParents_WhenNoRelationshipRowExists()
    {
        // Arrange
        var dc = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var appReader = dc.Loader.AppReaderRaw(TestSpecs.AppId, new());
        var templateEntity = appReader.List.First();

        // Find a content-type in this app which definitely has attributes.
        // We'll create a parent entity of that type and pick an attribute field from it.
        var selectedType = templateEntity.Type;
        var selectedContentTypeId = 0;

        foreach (var ent in appReader.List)
        {
            var ctId = dc.SqlDb.TsDynDataContentTypes
                .AsNoTracking()
                .Where(ct => ct.AppId == TestSpecs.AppId && ct.TransDeletedId == null && ct.StaticName == ent.Type.NameId)
                .Select(ct => ct.ContentTypeId)
                .FirstOrDefault();

            if (ctId <= 0)
                continue;

            var hasAttributes = dc.SqlDb.TsDynDataAttributes
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Any(a => a.TransDeletedId == null && a.ContentTypeId == ctId);

            if (!hasAttributes)
                continue;

            selectedType = ent.Type;
            selectedContentTypeId = ctId;
            break;
        }

        True(selectedContentTypeId > 0, "Expected to find at least one content-type in the scenario app with attributes.");

        var relationAttribute = dc.SqlDb.TsDynDataAttributes
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(a => a.TransDeletedId == null && a.ContentTypeId == selectedContentTypeId)
            .Select(a => new
            {
                a.AttributeId,
                Field = a.StaticName,
            })
            .FirstOrDefault();

        NotNull(relationAttribute);

        var attributeId = relationAttribute!.AttributeId;
        var fieldName = relationAttribute.Field;

        // Create a parent entity of that type.
        var parentGuid = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var parentEntity = dataBuilder.Entity.Create(
            appId: TestSpecs.AppId,
            contentType: selectedType,
            attributes: dataBuilder.Attribute.Create(selectedType, preparedValues: null),
            entityId: 0,
            repositoryId: 0,
            guid: parentGuid,
            titleField: selectedType.TitleFieldName,
            created: now,
            modified: now,
            owner: "test",
            version: 1,
            isPublished: true
        );

        importService
            .Init(zoneId: TestSpecs.ZoneId, appId: TestSpecs.AppId, skipExistingAttributes: false, preserveUntouchedAttributes: false)
            .ImportIntoDb([], [parentEntity]);

        var parentRows = dc.SqlDb.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.AppId == TestSpecs.AppId && e.EntityGuid == parentGuid)
            .Select(e => new { e.EntityId, e.IsPublished })
            .ToList();

        var publishedParent = parentRows.SingleOrDefault(r => r.IsPublished);
        True(publishedParent != null, $"Expected a published parent entity row for guid {parentGuid}, but none found.");

        var parentId = publishedParent!.EntityId;

        var maxSortOrder = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => r.ParentEntityId == parentId && r.AttributeId == attributeId)
            .Select(r => (int?)r.SortOrder)
            .Max() ?? 0;

        var newSortOrder = maxSortOrder + 10;

        var entityGuid = Guid.NewGuid();
        var sourceId = int.MaxValue - 124;
        var parentRef = DbVersioning.ParentRefForApp(TestSpecs.AppId);

        var txTimestamp = DateTime.UtcNow;
        var tx = new TsDynDataTransaction
        {
            Timestamp = txTimestamp,
            User = "history-parents-only-recycle-test",
        };
        dc.SqlDb.TsDynDataTransactions.Add(tx);
        dc.SqlDb.SaveChanges();

        // Create a minimal entity-json package with Parents like versioning would.
        var json = JsonSerializer.Serialize(
            new JsonFormat
            {
                Entity = new JsonEntity
                {
                    Id = sourceId,
                    Guid = entityGuid,
                    Version = 1,
                    Type = new JsonType { Id = selectedType.NameId, Name = selectedType.Name },
                    Attributes = new JsonAttributes(),
                    Parents =
                    [
                        new JsonRelationship
                        {
                            Parent = parentGuid,
                            Field = fieldName,
                            SortOrder = newSortOrder,
                        },
                    ],
                }
            },
            JsonOptions.UnsafeJsonWithoutEncodingHtml);

        var cJson = new Compressor().CompressOrNullIfDisabled(json);
        NotNull(cJson);

        dc.SqlDb.TsDynDataHistories.Add(new TsDynDataHistory
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
        });

        dc.SqlDb.SaveChanges();

        // Ensure there is no relationship row for this (parent, attribute, sortOrder) yet.
        var preExisting = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Any(r => r.ParentEntityId == parentId
                && r.AttributeId == attributeId
                && r.SortOrder == newSortOrder);
        False(preExisting);

        // Act
        var recycler = workEntityRecycle.New(appId: TestSpecs.AppId);
        recycler.Recycle(transactionId: tx.TransactionId);

        // Assert
        var dc2 = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        var restoredEntity = dc2.SqlDb.TsDynDataEntities
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Single(e => e.AppId == TestSpecs.AppId && e.EntityGuid == entityGuid);

        var rel = dc2.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Single(r => r.ParentEntityId == parentId
                && r.AttributeId == attributeId
                && r.SortOrder == newSortOrder);

        Equal(restoredEntity.EntityId, rel.ChildEntityId);
        Null(rel.ChildExternalId);
    }
}
