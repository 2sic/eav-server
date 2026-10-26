using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Testing.Scenarios;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Repository.Efc.Tests.DbParts;

[Startup(typeof(StartupTestsApps))]
public class DbAppTests(DbStorage dbData) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public void DeleteApp_RejectsExternalContentTypeReferences_BeforeDeletingData(bool fullDelete, bool softDeleted)
    {
        SetupStorage();
        using var transaction = dbData.SqlDb.Database.BeginTransaction();
        var template = dbData.SqlDb.TsDynDataEntities.IgnoreQueryFilters()
            .Select(e => new { e.TargetTypeId, e.TransCreatedId, e.TransModifiedId })
            .First();
        var parentAppId = CreateApp();
        var childAppId = CreateApp();
        var contentType = new TsDynDataContentType
        {
            AppId = parentAppId,
            StaticName = Guid.NewGuid().ToString(),
            Name = "Delete dependency test",
            TransCreatedId = template.TransCreatedId,
            TransDeletedId = softDeleted ? template.TransCreatedId : null,
        };
        dbData.SqlDb.TsDynDataContentTypes.Add(contentType);
        dbData.SqlDb.SaveChanges();
        foreach (var appId in new[] { parentAppId, childAppId })
            dbData.SqlDb.TsDynDataEntities.Add(new()
            {
                AppId = appId,
                ContentTypeId = contentType.ContentTypeId,
                EntityGuid = Guid.NewGuid(),
                Owner = "Delete dependency test",
                TargetTypeId = template.TargetTypeId,
                TransCreatedId = template.TransCreatedId,
                TransModifiedId = template.TransModifiedId,
                TransDeletedId = softDeleted ? template.TransCreatedId : null,
            });
        dbData.SqlDb.SaveChanges();

        var exception = Throws<InvalidOperationException>(() => dbData.App.DeleteApp(parentAppId, fullDelete));

        Equal($"Cannot delete or reset app {parentAppId} because its content types are still used by apps: {childAppId}. Remove those apps or resolve their dependencies first.", exception.Message);
        Equal(2, dbData.SqlDb.TsDynDataEntities.IgnoreQueryFilters().Count(e => e.ContentTypeId == contentType.ContentTypeId));
        True(dbData.SqlDb.TsDynDataContentTypes.IgnoreQueryFilters().Any(ct => ct.ContentTypeId == contentType.ContentTypeId));
        Equal(2, dbData.SqlDb.TsDynDataApps.IgnoreQueryFilters().Count(a => a.AppId == parentAppId || a.AppId == childAppId));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DeleteApp_AllowsDeletion_WithoutExternalReferences(bool fullDelete)
    {
        SetupStorage();
        using var transaction = dbData.SqlDb.Database.BeginTransaction();
        var appId = CreateApp();

        dbData.App.DeleteApp(appId, fullDelete);

        Equal(!fullDelete, dbData.SqlDb.TsDynDataApps.IgnoreQueryFilters().Any(a => a.AppId == appId));
    }

    private void SetupStorage()
    {
        var app = dbData.SqlDb.TsDynDataApps.Select(a => new { a.ZoneId, a.AppId }).First();
        dbData.Setup(new(app.ZoneId, app.AppId, null, new()));
    }

    private int CreateApp()
    {
        var app = new TsDynDataApp
        {
            ZoneId = dbData.SqlDb.TsDynDataZones.Select(z => z.ZoneId).First(),
            Name = Guid.NewGuid().ToString(),
        };
        dbData.SqlDb.TsDynDataApps.Add(app);
        dbData.SqlDb.SaveChanges();
        return app.AppId;
    }
}
