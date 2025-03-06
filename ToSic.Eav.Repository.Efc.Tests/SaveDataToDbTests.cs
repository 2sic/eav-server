using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Repository.Efc.Tests;

[TestClass]
public class SaveDataToDbTests: TestBaseDiEavFullAndDb
{
    public SaveDataToDbTests()
    {
        _dbData = GetService<DbDataController>();
        _loader1 = GetService<EfcAppLoader>();
        _loader2 = GetService<EfcAppLoader>();
        _environment = GetService<IImportExportEnvironment>();
        _entitySaver = GetService<EntitySaver>();
    }
    private readonly DbDataController _dbData;
    private readonly EfcAppLoader _loader1;
    private readonly EfcAppLoader _loader2;
    private readonly IImportExportEnvironment _environment;
    private readonly EntitySaver _entitySaver;


    [TestInitialize]
    public void Init()
    {
        Trace.Write("initializing DB & loader");
    }

    [TestMethod]
    public void LoadOneAndSaveUnchanged()
    {
        var test = new SpecsDataEditing();
        var so = _environment.SaveOptions(test.ZoneId);
        var dbi = _dbData.Init(test.ZoneId, test.AppId);
        var trans = dbi.SqlDb.Database.BeginTransaction();

        // load an entity
        var loader1 = _loader1.UseExistingDb(dbi.SqlDb);
        var app1 = loader1.AppStateReaderRawTA(test.AppId);
        var itm1 = app1.List.One(test.ExistingItem);

        // save it
        dbi.Save([itm1], so);

        // re-load it
        var loader2 = _loader2.UseExistingDb(dbi.SqlDb); // use existing db context because the transaction is still open
        var app2 = loader2.AppStateReaderRawTA(test.AppId);
        var itm2 = app2.List.One(test.ExistingItem);


        // validate that they are still the same!
        Assert.AreEqual(itm1.Attributes.Count, itm2.Attributes.Count, "should have same amount of attributes");

        trans.Rollback();
    }

    [TestMethod]
    public void LoadOneChangeABitAndSave()
    {
        var test = new SpecsDataEditing();
        var so = _environment.SaveOptions(test.ZoneId) with
        {
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
        };

        var dbi = _dbData.Init(test.ZoneId, test.AppId);
        var trans = dbi.SqlDb.Database.BeginTransaction();

        // todo: load a simple, 1 language entity
        var loader1 = _loader1.UseExistingDb(dbi.SqlDb);
        var app1 = loader1.AppStateReaderRawTA(test.AppId);
        var itm1 = app1.List.One(test.ExistingItem);

        // todo: make some minor changes
        var itmNewTitle = GetService<DataBuilder>().CreateEntityTac(appId: test.AppId, entityId: 0, contentType: itm1.Type, values: new()
        {
            {test.TitleField, "changed title on " + DateTime.Now}
        });
        var saveEntity = _entitySaver.TestCreateMergedForSavingTac(itm1, itmNewTitle, so);

        // save it
        dbi.Save([saveEntity], so);

        // reload it
        var loader2 = _loader2.UseExistingDb(dbi.SqlDb); // use existing db context because the transaction is still open
        var app2 = loader2.AppStateReaderRawTA(test.AppId);
        var itm2 = app2.List.One(test.ExistingItem);


        // todo: validate that they are almost the same, but clearly different
        Assert.AreEqual(itm1.Attributes.Count, itm2.Attributes.Count, "should have same amount of attributes");
        Assert.AreNotEqual(itm1.GetBestTitle(), itm2.GetBestTitle(), "title should have changed");

        trans.Rollback();
    }

    [TestMethod]
    public void CreateNewItemAndSave()
    {
        var test = new SpecsDataEditing();
        var ctName = test.ContentTypeName;
        var ctTitle = "wonderful new title " + DateTime.Now;
        var so = _environment.SaveOptions(test.ZoneId) with
        {
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
        };

        var dbi = _dbData.Init(test.ZoneId, test.AppId);
        var trans = dbi.SqlDb.Database.BeginTransaction();

        // load content type to start creating an item...
        var loader1 = _loader1.UseExistingDb(dbi.SqlDb);
        var app1 = loader1.AppStateReaderRawTA(test.AppId);
        var ct1 = app1.GetContentType(ctName);

        var newE = GetService<DataBuilder>().CreateEntityTac(appId: test.AppId, guid: Guid.NewGuid(), contentType: ct1, values: new()
        {
            { test.TitleField, ctTitle }
        });

        var saveEntity = _entitySaver.TestCreateMergedForSavingTac(null, newE, so);

        // save it
        var newId = dbi.Save([saveEntity], so);

        // reload it
        var loader2 = _loader2.UseExistingDb(dbi.SqlDb); // use existing db context because the transaction is still open
        var app2 = loader2.AppStateReaderRawTA(test.AppId);
        var itm2 = app2.List.One(newId.First());

        Assert.AreEqual(ctTitle, itm2.GetBestTitle(), "title should be loaded as saved" );

        trans.Rollback();


    }
}