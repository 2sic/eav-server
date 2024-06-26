﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.LookUp;

[TestClass]
public class LookUpEngineTests: TestBaseEavCore
{

    #region Important Values which will be checked when resolved
    public string OriginalSettingDefaultCat = "[AppSettings:DefaultCategoryName]";
    private readonly string ResolvedSettingDefaultCat = "All";
    private readonly string OriginalSettingMaxItems = "[AppSettings:MaxItems||100]";
    public static string ResolvedSettingMaxItems = "100";
    public static string MaxPictures = "21";
    public static string DefaultCategory = "All";
    #endregion

    [TestMethod]
    public void LookUpEngine_SourcesWork()
    {
        var lookUpEngine = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes();
        AssertLookUpEngineHasSourcesOfOriginal(lookUpEngine);
    }

    private void AssertLookUpEngineHasSourcesOfOriginal(ILookUpEngine lookUpEngine)
    {
        var settings = Settings();
        Assert.IsTrue(lookUpEngine.Sources.Count() == 2, "Should have 2 sources");
        Assert.AreEqual("App Settings", lookUpEngine.Sources.ToList().GetSource("appsettings").Get(Attributes.TitleNiceName));
        Assert.AreEqual(OriginalSettingDefaultCat, settings["DefaultCategory"]);
        Assert.AreEqual(OriginalSettingMaxItems, settings["MaxItems"]);
    }

    [TestMethod]
    public void LookUpEngine_DontModifyThingsWithoutTokens()
    {
        var vc = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes();
        var settings = Settings();
        settings = vc.LookUp(settings);
        Assert.AreEqual(ResolvedSettingDefaultCat, settings["DefaultCategory"], "Default should be all");
        Assert.AreEqual(ResolvedSettingMaxItems, settings["MaxItems"], "Max should be 100");
    }

    [TestMethod]
    public void BasicLookupsWork()
    {
        var mainEngine = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes(-1);
        var settings = mainEngine.LookUp(TestTokens());
        foreach (var setting in settings)
            Assert.AreEqual(setting.Key, setting.Value,
                $"expected '{setting.Key}', got '{setting.Value}'");
    }

    [TestMethod]
    public void OverrideLookUps()
    {
        var mainEngine = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes(-1);
        //var overrideSources = new DictionaryInvariant<ILookUp>();
        const string overridenTitle = "overriden Title";
        var overrideDic = new Dictionary<string, string>
        {
            {Attributes.TitleNiceName, overridenTitle}
        };
        var appSettingsSource = new LookUpInDictionary(LookUpTestData.KeyAppSettings, overrideDic);
        //overrideSources.Add(appSettingsSource.Name, appSettingsSource);
        // test before override
        var result = mainEngine.LookUp(TestTokens(), new List<ILookUp> { appSettingsSource });
        Assert.AreEqual(overridenTitle, result["App Settings"], "should override");
    }

    [TestMethod]
    public void InheritEngine()
    {
        var original = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes();
        var cloned = new LookUpEngine(original, null);
        Assert.AreEqual(0, cloned.Sources.Count());
        AssertLookUpEngineHasSourcesOfOriginal(cloned.Downstream);
    }




    public IDictionary<string, string> Settings() =>
        new Dictionary<string, string>
        {
            {Attributes.TitleNiceName, "Settings"},
            {"DefaultCategory", OriginalSettingDefaultCat},
            {"MaxItems", OriginalSettingMaxItems},
            {"PicsPerRow", "3"}
        };

    public IDictionary<string, string> TestTokens() =>
        new Dictionary<string, string>
        {
            {"App Settings", "[AppSettings:Title]"},
            {DefaultCategory, "[AppSettings:DefaultCategoryName]"},
            {$"!{DefaultCategory}!", "![AppSettings:DefaultCategoryName]!"},
            {$"xyz{DefaultCategory}123", "xyz[AppSettings:DefaultCategoryName]123"},
            {$" {DefaultCategory} ", " [AppSettings:DefaultCategoryName] "},
            {MaxPictures, "[AppSettings:MaxPictures]"},
            {"3", "[AppSettings:PicsPerRow]"},

            // incomplete token, don't change
            {"[AppResources:Title", "[AppResources:Title" },
            {"Resources", "[AppResources:Title]" },
            {"", "[AppResources:unknown]" },

        };

    //public IDictionary<string, string> ResolvedSettings()
    //{
    //    var settings = Settings();
    //    LookUpTestData.AppSetAndRes().LookUp(settings);
    //    return settings;
    //}
}