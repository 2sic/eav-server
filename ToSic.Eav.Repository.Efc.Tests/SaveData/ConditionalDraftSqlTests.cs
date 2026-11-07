using ToSic.Eav.Data.Build;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Repositories;
using ToSic.Sys.Boot;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Efc.Sys.Services;
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

[Startup(typeof(StartupTestsApps))]
public class ConditionalDraftSqlTests(DbStorage storage, Generator<EfcAppLoaderService> loaders, IImportExportEnvironment environment,
    EntitySaver saver, DataAssembler assembler, FixtureStartupWithDb fixture, BootCoordinator boot)
{
    [C3SqlFact]
    public void Save_UpdatesExistingDraft_AndPreservesPublishedRow()
    {
        fixture.SetupFixtureConfiguration(new ScenarioConditionalDraftSql());
        boot.StartUp();
        var appId = int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_APP")!);
        var zoneId = int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_ZONE")!);
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
        var rows = storage.SqlDb.TsDynDataEntities.AsNoTracking().Where(e => e.AppId == appId && e.EntityGuid == guid && e.TransDeletedId == null).ToArray();
        var published = rows.Single(e => e.EntityId == entityId && e.IsPublished);
        var draft = rows.Single(e => e.PublishedEntityId == entityId && !e.IsPublished);
        var expected = new ExistingDraftSaveCondition(appId, entityId, guid, published.Version, draft.EntityId, draft.Version);
        var app = loaders.New().UseExistingDb(storage.SqlDb).AppStateReaderRawTac(appId);
        var original = app.List.Single(e => e.EntityGuid == guid && !e.IsPublished);
        var options = environment.SaveOptions(zoneId) with
        {
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
            DraftShouldBranch = false,
            ExistingDraftCondition = expected,
        };
        var title = "C3 conditional draft " + Guid.NewGuid().ToString("N");
        IEntity partial = assembler.CreateEntityTac(appId: appId, entityId: 0, contentType: original.Type,
            values: new() { { "Title", title } });
        partial = assembler.Entity.CreateFrom(partial, isPublished: false);
        var merged = saver.TestCreateMergedForSavingTac(original, partial, options);
        Assert.False(merged.IsPublished);

        storage.Save(options.AddToAll([merged]));

        var after = storage.SqlDb.TsDynDataEntities.AsNoTracking().Where(e => e.AppId == appId && e.EntityGuid == guid && e.TransDeletedId == null).ToArray();
        var publishedAfter = after.Single(e => e.EntityId == entityId);
        var draftAfter = after.Single(e => e.EntityId == draft.EntityId);
        Assert.Equal(published.Version, publishedAfter.Version);
        Assert.Equal(published.Json, publishedAfter.Json);
        Assert.Equal(published.TransModifiedId, publishedAfter.TransModifiedId);
        Assert.Equal(draft.Version + 1, draftAfter.Version);
        Assert.False(draftAfter.IsPublished);
        Assert.Equal(entityId, draftAfter.PublishedEntityId);
        var readback = loaders.New().UseExistingDb(storage.SqlDb).AppStateReaderRawTac(appId).List.Single(e => e.EntityGuid == guid && !e.IsPublished);
        Assert.Equal(title, readback.GetBestTitle());
    }
}
