using System.Collections.Generic;
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
    private readonly string ResolvedSettingDefaultCat = "All";
    private readonly string OriginalSettingMaxItems = "[AppSettings:MaxItems||100]";
    private static string ResolvedSettingMaxItems = "100";

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
        Assert.AreEqual("App Settings", lookUpEngine.Sources.ToList().GetSource("appsettings").GetTac(Attributes.TitleNiceName));
        Assert.AreEqual(LookUpTestConstants.OriginalSettingDefaultCat, settings["DefaultCategory"]);
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
            Assert.AreEqual(setting.Key, setting.Value, $"expected '{setting.Key}', got '{setting.Value}'");
    }

    [TestMethod]
    public void BasicLookupsWithTweaks()
    {
        var mainEngine = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes(-1);
        var settings = mainEngine.LookUp(TestTokens("-tweaked"), tweak: t => t.PostProcess(v => v + "-tweaked"));
        foreach (var setting in settings)
            Assert.AreEqual(setting.Key, setting.Value, $"expected '{setting.Key}', got '{setting.Value}'");
    }

    [TestMethod]
    public void OverrideLookUps()
    {
        var mainEngine = new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes(-1);
        const string overridenTitle = "overriden Title";
        var overrideDic = new Dictionary<string, string>
        {
            {Attributes.TitleNiceName, overridenTitle}
        };
        var appSettingsSource = new LookUpInDictionary(LookUpTestConstants.KeyAppSettings, overrideDic);
        // test before override
        var result = mainEngine.LookUp(TestTokens(), overrides: new List<ILookUp> { appSettingsSource });
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




    private IDictionary<string, string> Settings() =>
        new Dictionary<string, string>
        {
            { Attributes.TitleNiceName, "Settings" },
            { "DefaultCategory", LookUpTestConstants.OriginalSettingDefaultCat },
            { "MaxItems", OriginalSettingMaxItems },
            { "PicsPerRow", "3" }
        };

    /// <summary>
    /// Get some test tokens with the values that should result
    /// </summary>
    /// <param name="tweakSuffix">For tweak testing, suffix added to value...</param>
    /// <returns></returns>
    private IDictionary<string, string> TestTokens(string tweakSuffix = default) =>
        // The dictionary - keys are the expected result, value are the tokens to use...
        new Dictionary<string, string>
        {
            { "App Settings" + tweakSuffix, "[AppSettings:Title]" },
            { LookUpTestConstants.DefaultCategory + tweakSuffix, "[AppSettings:DefaultCategoryName]" },
            { $"!{LookUpTestConstants.DefaultCategory}{tweakSuffix}!", "![AppSettings:DefaultCategoryName]!" },
            { $"xyz{LookUpTestConstants.DefaultCategory}{tweakSuffix}123", "xyz[AppSettings:DefaultCategoryName]123" },
            { $" {LookUpTestConstants.DefaultCategory}{tweakSuffix} ", " [AppSettings:DefaultCategoryName] " },
            { LookUpTestConstants.MaxPictures + tweakSuffix, "[AppSettings:MaxPictures]" },
            { "3" + tweakSuffix, "[AppSettings:PicsPerRow]" },
            { "Resources" + tweakSuffix, "[AppResources:Title]" },

            // incomplete token, don't change
            { "[AppResources:Title", "[AppResources:Title" },
            { "", "[AppResources:unknown]" },
        };
}