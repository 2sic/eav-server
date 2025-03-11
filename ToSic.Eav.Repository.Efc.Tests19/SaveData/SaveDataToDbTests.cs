using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Tests;
using ToSic.Eav.Repositories;
using ToSic.Eav.Testing.Scenarios;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Repository.Efc.Tests;

[Startup(typeof(StartupTestFullWithDb))]
public class SaveDataToDbTests(DbDataController dbData, EfcAppLoader _loader1, EfcAppLoader _loader2, IImportExportEnvironment environment, EntitySaver entitySaver, DataBuilder dataBuilder)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    //public SaveDataToDbTests()
    //{
    //    _dbData = GetService<DbDataController>();
    //    _loader1 = GetService<EfcAppLoader>();
    //    _loader2 = GetService<EfcAppLoader>();
    //    _environment = GetService<IImportExportEnvironment>();
    //    _entitySaver = GetService<EntitySaver>();
    //}
    //private readonly DbDataController _dbData;
    //private readonly EfcAppLoader _loader1;
    //private readonly EfcAppLoader _loader2;
    //private readonly IImportExportEnvironment _environment;
    //private readonly EntitySaver _entitySaver;


    //[TestInitialize]
    //public void Init()
    //{
    //    Trace.Write("initializing DB & loader");
    //}

    [Fact]
    public void LoadOneAndSaveUnchanged()
    {
        var test = new SpecsDataEditing();
        var so = environment.SaveOptions(test.ZoneId);
        var dbi = dbData.Init(test.ZoneId, test.AppId);
        var trans = dbi.SqlDb.Database.BeginTransaction();

        // load an entity
        var loader1 = _loader1.UseExistingDb(dbi.SqlDb);
        var app1 = loader1.AppStateReaderRawTac(test.AppId);
        var itm1 = app1.List.One(test.ExistingItem);

        // save it
        dbi.Save([itm1], so);

        // re-load it
        var loader2 = _loader2.UseExistingDb(dbi.SqlDb); // use existing db context because the transaction is still open
        var app2 = loader2.AppStateReaderRawTac(test.AppId);
        var itm2 = app2.List.One(test.ExistingItem);


        // validate that they are still the same!
        Equal(itm1.Attributes.Count, itm2.Attributes.Count);//, "should have same amount of attributes");

        trans.Rollback();
    }

    [Fact]
    public void LoadOneChangeABitAndSave()
    {
        var test = new SpecsDataEditing();
        var so = environment.SaveOptions(test.ZoneId) with
        {
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
        };

        var dbi = dbData.Init(test.ZoneId, test.AppId);
        var trans = dbi.SqlDb.Database.BeginTransaction();

        // todo: load a simple, 1 language entity
        var loader1 = _loader1.UseExistingDb(dbi.SqlDb);
        var app1 = loader1.AppStateReaderRawTac(test.AppId);
        var itm1 = app1.List.One(test.ExistingItem);

        // todo: make some minor changes
        var itmNewTitle = dataBuilder.CreateEntityTac(appId: test.AppId, entityId: 0, contentType: itm1.Type, values: new()
        {
            {test.TitleField, "changed title on " + DateTime.Now}
        });
        var saveEntity = entitySaver.TestCreateMergedForSavingTac(itm1, itmNewTitle, so);

        // save it
        dbi.Save([saveEntity], so);

        // reload it
        var loader2 = _loader2.UseExistingDb(dbi.SqlDb); // use existing db context because the transaction is still open
        var app2 = loader2.AppStateReaderRawTac(test.AppId);
        var itm2 = app2.List.One(test.ExistingItem);


        // todo: validate that they are almost the same, but clearly different
        Equal(itm1.Attributes.Count, itm2.Attributes.Count);//, "should have same amount of attributes");
        NotEqual(itm1.GetBestTitle(), itm2.GetBestTitle());//, "title should have changed");

        trans.Rollback();
    }

    [Fact]
    public void CreateNewItemAndSave()
    {
        var test = new SpecsDataEditing();
        var ctName = test.ContentTypeName;
        var ctTitle = "wonderful new title " + DateTime.Now;
        var so = environment.SaveOptions(test.ZoneId) with
        {
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
        };

        var dbi = dbData.Init(test.ZoneId, test.AppId);
        var trans = dbi.SqlDb.Database.BeginTransaction();

        // load content type to start creating an item...
        var loader1 = _loader1.UseExistingDb(dbi.SqlDb);
        var app1 = loader1.AppStateReaderRawTac(test.AppId);
        var ct1 = app1.GetContentType(ctName);

        var newE = dataBuilder.CreateEntityTac(appId: test.AppId, guid: Guid.NewGuid(), contentType: ct1, values: new()
        {
            { test.TitleField, ctTitle }
        });

        var saveEntity = entitySaver.TestCreateMergedForSavingTac(null, newE, so);

        // save it
        var newId = dbi.Save([saveEntity], so);

        // reload it
        var loader2 = _loader2.UseExistingDb(dbi.SqlDb); // use existing db context because the transaction is still open
        var app2 = loader2.AppStateReaderRawTac(test.AppId);
        var itm2 = app2.List.One(newId.First());

        Equal(ctTitle, itm2.GetBestTitle());//, "title should be loaded as saved" );

        trans.Rollback();


    }
}