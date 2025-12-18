using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Testing.Scenarios;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Versioning;

[Startup(typeof(StartupTestWork))]
public class DeleteHistoryInboundParentsTests(
    Generator<DbStorage, StorageOptions> dbDataGenerator,
    GenWorkDb<WorkEntityDelete> workEntityDelete,
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
    public void Delete_SavesHistoryWithInboundParents_WhenRelationshipsWillBeRemoved()
    {
        // Arrange
        var dc = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));

        // Pick an existing relationship to get a valid parent + attribute/field.
        var templateRel = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .Where(r => r.ChildEntityId != null)
            .Select(r => new
            {
                r.ParentEntityId,
                ParentGuid = r.ParentEntity.EntityGuid,
                r.AttributeId,
                Field = r.Attribute.StaticName,
            })
            .FirstOrDefault();

        NotNull(templateRel);

        // Create a new entity to delete (so we don't mutate scenario-fixed entities).
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
            .ImportIntoDb([], new List<Entity> { newChild });

        var childId = dc.SqlDb.TsDynDataEntities
            .AsNoTracking()
            .Where(e => e.AppId == TestSpecs.AppId && e.EntityGuid == newGuid && e.TransDeletedId == null)
            .Select(e => e.EntityId)
            .Single();

        // Important: PK on TsDynDataRelationship includes (ParentEntityId, AttributeId, SortOrder).
        var nextSortOrder = dc.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => r.ParentEntityId == templateRel.ParentEntityId && r.AttributeId == templateRel.AttributeId)
            .Select(r => (int?)r.SortOrder)
            .Max() ?? 0;
        nextSortOrder++;

        dc.SqlDb.TsDynDataRelationships.Add(new()
        {
            AttributeId = templateRel.AttributeId,
            ParentEntityId = templateRel.ParentEntityId,
            ChildEntityId = childId,
            SortOrder = nextSortOrder,
        });
        dc.SqlDb.SaveChanges();

        // Act
        var deleter = workEntityDelete.New(appId: TestSpecs.AppId);
        var ok = deleter.Delete(id: childId, force: true);

        // Assert
        True(ok);

        // Re-open a fresh DbStorage to avoid any EF tracking surprises.
        var dc2 = dbDataGenerator.New(new(TestSpecs.ZoneId, TestSpecs.AppId));
        var history = dc2.Versioning.GetHistoryList(childId, includeData: true);
        NotEmpty(history);

        var latest = history.First();
        NotNull(latest.Json);

        var format = System.Text.Json.JsonSerializer.Deserialize<JsonFormat>(latest.Json!);
        NotNull(format);
        NotNull(format!.Entity);

        var parents = format.Entity!.Parents;
        NotNull(parents);

        Contains(parents!, p =>
            p.Parent == templateRel.ParentGuid
            && p.Field == templateRel.Field
            && p.SortOrder == nextSortOrder
        );
    }
}
