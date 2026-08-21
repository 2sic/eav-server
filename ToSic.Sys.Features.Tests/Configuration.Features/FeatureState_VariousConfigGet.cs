using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Features.Configuration.Features;

// ReSharper disable once InconsistentNaming
public class FeatureState_VariousConfigGet
{
    private const string KeyWithTrue = "logDataLoading";
    private const string KeyWithFalse = "logDataLoadingFalse";
    private const string KeyWithInt = "logDataLoadingInt";
    private const string KeyWithString = "logDataLoadingString";
    private const string KeyNothing = "nothing";

    private static FeatureStateTestObject TestFeatureStateData =>
        new(Feature.UnknownFeature(new()), DateTime.Now, true, "", "",
            true, true, true,
            new()
            {
                [KeyWithTrue] = true,
                [KeyWithFalse] = false,
                [KeyWithInt] = 27,
                [KeyWithString] = "message",
            });

    [Fact]
    public void ConfigBool_True_AndExplicitDefaultFalse()
        // ReSharper disable once RedundantArgumentDefaultValue
        => True(TestFeatureStateData.ConfigBoolTac(KeyWithTrue, false));
    [Fact]
    public void ConfigBool_True()
        => True(TestFeatureStateData.ConfigBoolTac(KeyWithTrue));

    [Fact]
    public void ConfigBool_False()
        => False(TestFeatureStateData.ConfigBoolTac(KeyWithFalse));
    
    [Fact]
    public void ConfigBool_Missing_DefaultsToFalse()
        => False(TestFeatureStateData.ConfigBoolTac(KeyNothing));

    [Fact]
    public void ConfigBool_Missing_ExplicitFallback()
        => True(TestFeatureStateData.ConfigBoolTac(KeyNothing, true));

    [Fact]
    public void ConfigInt_Existing()
        => Equal(27, TestFeatureStateData.ConfigIntTac(KeyWithInt));

    [Fact]
    public void ConfigInt_Existing_Fallback_ReturnsExisting()
        => Equal(27, TestFeatureStateData.ConfigIntTac(KeyWithInt, 1));

    [Fact]
    public void ConfigInt_Missing_Zero()
        => Equal(0, TestFeatureStateData.ConfigIntTac(KeyNothing));

    [Fact]
    public void ConfigInt_Missing_WithFallback()
        => Equal(9, TestFeatureStateData.ConfigIntTac(KeyNothing, 9));



    [Fact]
    public void ConfigString_Existing()
        => Equal("message", TestFeatureStateData.ConfigStringTac(KeyWithString));

    [Fact]
    public void ConfigString_Existing_Fallback_ReturnsExisting()
        => Equal("message", TestFeatureStateData.ConfigStringTac(KeyWithString, "ok"));

    [Fact]
    public void ConfigString_Missing_Null()
        => Null(TestFeatureStateData.ConfigStringTac(KeyNothing));

    [Fact]
    public void ConfigString_Missing_WithFallback()
        => Equal("empty", TestFeatureStateData.ConfigStringTac(KeyNothing, "empty"));


}