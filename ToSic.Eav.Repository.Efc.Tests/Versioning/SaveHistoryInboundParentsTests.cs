using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Testing.Scenarios;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Repository.Efc.Tests.Versioning;

[Startup(typeof(StartupTestsApps))]
public class SaveHistoryInboundParentsTests(
    DbStorage dbData,
    Generator<EfcAppLoaderService> appLoadGenerator,
    IImportExportEnvironment environment,
    EntitySaver entitySaver,
    DataBuilder dataBuilder)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void Save_IncludesInboundParentsInHistoryJson()
    {
        var test = new SaveData.SpecsDataEditing();
        var so = environment.SaveOptions(test.ZoneId) with
        {
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
        };

        dbData.Setup(new(test.ZoneId, test.AppId, null, new()));
        var transaction = dbData.SqlDb.Database.BeginTransaction();

        // Create parent + child and connect them using a real Entity-type attribute.
        // This avoids depending on pre-existing relationships in the fixture DB.
        var loader1 = appLoadGenerator.New().UseExistingDb(dbData.SqlDb);
        var app1 = loader1.AppStateReaderRawTac(test.AppId);

        var titleAttribute = dbData.SqlDb.TsDynDataAttributes
            .AsNoTracking()
            .Where(a => a.TransDeletedId == null)
            .Where(a => a.ContentType.TransDeletedId == null)
            .Where(a => a.ContentType.AppId == test.AppId)
            .Where(a => a.IsTitle)
            .Select(a => new
            {
                TitleField = a.StaticName,
                CtNameId = a.ContentType.StaticName,
                a.ContentTypeId,
            })
            .FirstOrDefault();

        NotNull(titleAttribute);

        var relationAttribute = dbData.SqlDb.TsDynDataAttributes
            .AsNoTracking()
            .Where(a => a.TransDeletedId == null)
            .Where(a => a.ContentTypeId == titleAttribute!.ContentTypeId)
            .Select(a => new
            {
                a.AttributeId,
                Field = a.StaticName,
            })
            .FirstOrDefault();

        NotNull(relationAttribute);

        var titleField = titleAttribute!.TitleField;
        var ct = app1.GetContentTypeTac(titleAttribute!.CtNameId);

        var parentGuid = Guid.NewGuid();
        var parentEntity = dataBuilder.CreateEntityTac(appId: test.AppId, guid: parentGuid, contentType: ct, values: new()
        {
            { titleField, "history parents parent " + DateTime.UtcNow }
        });
        var saveParent = entitySaver.TestCreateMergedForSavingTac(null, parentEntity, so);
        var parentId = dbData.Save(so.AddToAll([saveParent])).First().Id;

        var childGuid = Guid.NewGuid();
        var childEntity = dataBuilder.CreateEntityTac(appId: test.AppId, guid: childGuid, contentType: ct, values: new()
        {
            { titleField, "history parents child " + DateTime.UtcNow }
        });
        var saveChild = entitySaver.TestCreateMergedForSavingTac(null, childEntity, so);
        var childId = dbData.Save(so.AddToAll([saveChild])).First().Id;

        // Relationship PK is (AttributeId, ParentEntityId, SortOrder) and ignores TransDeletedId.
        var nextSortOrder = dbData.SqlDb.TsDynDataRelationships
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => r.ParentEntityId == parentId && r.AttributeId == relationAttribute!.AttributeId)
            .Select(r => (int?)r.SortOrder)
            .Max() ?? 0;
        nextSortOrder++;

        dbData.SqlDb.TsDynDataRelationships.Add(new()
        {
            AttributeId = relationAttribute!.AttributeId,
            ParentEntityId = parentId,
            ChildEntityId = childId,
            SortOrder = nextSortOrder,
            TransDeletedId = null,
        });
        dbData.SqlDb.SaveChanges();

        // Save again to generate a new history snapshot which should now include Parents.
        var loader2 = appLoadGenerator.New().UseExistingDb(dbData.SqlDb);
        var app2 = loader2.AppStateReaderRawTac(test.AppId);
        var existing = app2.List.One(childId)!;

        var update = dataBuilder.CreateEntityTac(appId: test.AppId, entityId: 0, contentType: existing.Type, values: new()
        {
            { titleField, "changed title " + DateTime.UtcNow }
        });

        var saveEntity2 = entitySaver.TestCreateMergedForSavingTac(existing, update, so);
        dbData.Save(so.AddToAll([saveEntity2]));

        var history = dbData.Versioning.GetHistoryList(childId, includeData: true);
        NotEmpty(history);

        var latest = history.First();
        NotNull(latest.Json);

        var format = System.Text.Json.JsonSerializer.Deserialize<JsonFormat>(latest.Json!);
        NotNull(format);
        NotNull(format!.Entity);

        var parents = format.Entity!.Parents;
        NotNull(parents);

        Contains(parents!, p =>
            p.Parent == parentGuid
            && p.Field == relationAttribute!.Field
            && p.SortOrder == nextSortOrder
        );

        transaction.Rollback();
    }
}
