using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Sys.Boot;

namespace ToSic.Eav.Repository.Efc.Tests.SaveData;

// Shared by persistence and Work tests; no setup or database access until Prepare is explicitly called.
public sealed class ConditionalDraftSqlFixture(DbStorage storage, Generator<EfcAppLoaderService> loaders,
    IImportExportEnvironment environment, EntitySaver saver, DataAssembler assembler,
    FixtureStartupWithDb fixture, BootCoordinator boot, Generator<DbStorage> stores)
{
    private bool _ownershipVerified;
    public static int ZoneId => int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_ZONE")!);

    public ExistingDraftSaveCondition Prepare()
    {
        fixture.SetupFixtureConfiguration(new ScenarioConditionalDraftSql());
        boot.StartUp();
        var appId = int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_APP")!);
        var entityId = int.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_ENTITY")!);
        var guid = Guid.Parse(System.Environment.GetEnvironmentVariable("SXC_C3_GUID")!);
        storage.Setup(new(ZoneId, appId, null, new()));
        Assert.StartsWith("SxcMcpC3_", storage.SqlDb.Database.GetDbConnection().Database);
        storage.SqlDb.Database.OpenConnection();
        using (var marker = storage.SqlDb.Database.GetDbConnection().CreateCommand())
        {
            marker.CommandText = "SELECT CONVERT(nvarchar(64),value) FROM sys.extended_properties WHERE class=0 AND name=N'2sxc-mcp-c3-owner'";
            Assert.Equal(System.Environment.GetEnvironmentVariable("SXC_C3_NONCE"), marker.ExecuteScalar());
        }
        _ownershipVerified = true;
        // The generic test host does not run DNN's on-demand language synchronization.
        EnsureLanguage("en-us", "English (C3 SQL fixture)");
        var rows = storage.SqlDb.TsDynDataEntities.AsNoTracking()
            .Where(e => e.AppId == appId && e.EntityGuid == guid && e.TransDeletedId == null).ToArray();
        var published = rows.Single(e => e.EntityId == entityId && e.IsPublished);
        var draft = rows.Single(e => e.PublishedEntityId == entityId && !e.IsPublished);
        return new(appId, entityId, guid, published.Version, draft.EntityId, draft.Version);
    }

    public void EnsureLanguage(string code, string name)
    {
        Assert.True(_ownershipVerified);
        var dimensions = stores.New();
        dimensions.Setup(new(ZoneId, storage.AppId, null, new()));
        using (dimensions.SqlDb)
            if (!dimensions.SqlDb.TsDynDataDimensions.Any(d => d.ZoneId == ZoneId && d.EnvironmentKey == code))
                dimensions.Dimensions.AddOrUpdateZoneDimension(code, name, true);
    }

    public (IEntity Entity, SaveOptions Options) PrepareTitle(DbStorage target, ExistingDraftSaveCondition expected,
        string title, bool conditional = true)
    {
        var app = loaders.New().UseExistingDb(target.SqlDb).AppStateReaderRawTac(expected.AppId);
        var original = app.List.Single(e => e.EntityGuid == expected.EntityGuid && !e.IsPublished);
        var options = environment.SaveOptions(ZoneId) with
        {
            Languages = target.SqlDb.TsDynDataDimensions.AsNoTracking().Include(d => d.ParentNavigation)
                .Where(d => d.ZoneId == ZoneId && d.Parent != null).ToList().Select(d => d.AsDimensionDefinition).ToList(),
            PreserveUntouchedAttributes = true,
            PreserveExistingLanguages = true,
            PreserveUnknownLanguages = true,
            DraftShouldBranch = !conditional,
            ExistingDraftCondition = conditional ? expected : null,
        };
        var titleAttribute = assembler.Attribute.CreateOrUpdate(null, "Title", title, ValueTypes.String, language: "en-us");
        IEntity partial = assembler.CreateEntityTac(appId: expected.AppId, entityId: 0, contentType: original.Type,
            typedValues: new() { { "Title", titleAttribute } }, isPublished: false);
        var merged = MergePartial(original, partial, options);
        Assert.False(merged.IsPublished);
        return (merged, options);
    }

    public IEntity MergePartial(IEntity original, IEntity partial, SaveOptions options)
        => saver.TestCreateMergedForSavingTac(original, partial, options);
}
