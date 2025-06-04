using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys;
using ToSic.Lib.LookUp;
using ToSic.Lib.LookUp.Engines;
using ToSic.Lib.LookUp.Sources;
using Xunit.DependencyInjection;

namespace ToSic.Eav.LookUp;

[Startup(typeof(StartupTestsEavCore))]
public class LookUpEngineTests(DataBuilder dataBuilder)
{
    #region Constants

    private const string ResolvedSettingDefaultCat = "All";
    private const string OriginalSettingMaxItems = "[AppSettings:MaxItems||100]";
    private const string ResolvedSettingMaxItems = "100";

    #endregion

    [Fact]
    public void LookUpEngine_SourcesWork()
    {
        var lookUpEngine = new LookUpTestData(dataBuilder).AppSetAndRes();
        AssertLookUpEngineHasSourcesOfOriginal(lookUpEngine);
    }

    private void AssertLookUpEngineHasSourcesOfOriginal(ILookUpEngine lookUpEngine)
    {
        var settings = Settings();
        True(lookUpEngine.Sources.Count() == 2, "Should have 2 sources");
        Equal("App Settings", lookUpEngine.Sources.ToList().GetSource("appsettings").GetTac(AttributeNames.TitleNiceName));
        Equal(LookUpTestConstants.OriginalSettingDefaultCat, settings["DefaultCategory"]);
        Equal(OriginalSettingMaxItems, settings["MaxItems"]);
    }

    [Fact]
    public void LookUpEngine_DontModifyThingsWithoutTokens()
    {
        var vc = new LookUpTestData(dataBuilder).AppSetAndRes();
        var settings = Settings();
        settings = vc.LookUp(settings);
        Equal(ResolvedSettingDefaultCat, settings["DefaultCategory"]); //, "Default should be all");
        Equal(ResolvedSettingMaxItems, settings["MaxItems"]); //, "Max should be 100");
    }

    [Fact]
    public void BasicLookupsWork()
    {
        var mainEngine = new LookUpTestData(dataBuilder).AppSetAndRes(-1);
        var settings = mainEngine.LookUp(TestTokens());
        foreach (var setting in settings)
            Equal(setting.Key, setting.Value); //, $"expected '{setting.Key}', got '{setting.Value}'");
    }

    [Fact]
    public void BasicLookupsWithTweaks()
    {
        var mainEngine = new LookUpTestData(dataBuilder).AppSetAndRes(-1);
        var settings = mainEngine.LookUp(TestTokens("-tweaked"), tweak: t => t.PostProcess(v => v + "-tweaked"));
        foreach (var setting in settings)
            Equal(setting.Key, setting.Value); //, $"expected '{setting.Key}', got '{setting.Value}'");
    }

    [Fact]
    public void OverrideLookUps()
    {
        var mainEngine = new LookUpTestData(dataBuilder).AppSetAndRes(-1);
        const string overridenTitle = "overriden Title";
        var overrideDic = new Dictionary<string, string>
        {
            {AttributeNames.TitleNiceName, overridenTitle}
        };
        var appSettingsSource = new LookUpInDictionary(LookUpTestConstants.KeyAppSettings, overrideDic);
        // test before override
        var result = mainEngine.LookUp(TestTokens(), overrides: new List<ILookUp> { appSettingsSource });
        Equal(overridenTitle, result["App Settings"]); //, "should override");
    }

    [Fact]
    public void InheritEngine()
    {
        var original = new LookUpTestData(dataBuilder).AppSetAndRes();
        var cloned = new LookUpEngine(original, null);
        Empty(cloned.Sources);
        AssertLookUpEngineHasSourcesOfOriginal(cloned.Downstream);
    }




    private IDictionary<string, string> Settings() =>
        new Dictionary<string, string>
        {
            { AttributeNames.TitleNiceName, "Settings" },
            { "DefaultCategory", LookUpTestConstants.OriginalSettingDefaultCat },
            { "MaxItems", OriginalSettingMaxItems },
            { "PicsPerRow", "3" }
        };

    /// <summary>
    /// Get some test tokens with the values that should result
    /// </summary>
    /// <param name="tweakSuffix">For tweak testing, suffix added to value...</param>
    /// <returns></returns>
    private Dictionary<string, string> TestTokens(string? tweakSuffix = null) =>
        // The dictionary - keys are the expected result, value are the tokens to use...
        new()
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