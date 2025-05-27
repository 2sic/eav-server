using ToSic.Eav.SysData;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Configuration.Features;

public class FeatureStateConfiguration
{
    private const string KeyWithTrue = "logDataLoading";
    private const string KeyWithFalse = "logDataLoadingFalse";
    private const string KeyWithInt = "logDataLoadingInt";
    private const string KeyWithString = "logDataLoadingString";
    private const string KeyNothing = "nothing";

    [Fact]
    public void FeatureStateWithConfigBoolTrueAndExplicitDefaultFalse()
        // ReSharper disable once RedundantArgumentDefaultValue
        => True(TestFeatureStateData.ConfigBool(KeyWithTrue, false));
    [Fact]
    public void FeatureStateWithConfigBoolTrue()
        => True(TestFeatureStateData.ConfigBool(KeyWithTrue));

    [Fact]
    public void FeatureStateWithConfigBoolFalse()
        => False(TestFeatureStateData.ConfigBool(KeyWithFalse));
    
    [Fact]
    public void FeatureStateWithConfigBoolMissingFalse()
        => False(TestFeatureStateData.ConfigBool(KeyNothing));

    [Fact]
    public void FeatureStateWithConfigBoolMissingTrue()
        => True(TestFeatureStateData.ConfigBool(KeyNothing, true));

    [Fact]
    public void FeatureStateWithConfigInt()
        => Equal(27, TestFeatureStateData.ConfigInt(KeyWithInt));

    [Fact]
    public void FeatureStateWithConfigIntDefault1()
        => Equal(27, TestFeatureStateData.ConfigInt(KeyWithInt, 1));

    [Fact]
    public void FeatureStateWithConfigIntMissing()
        => Equal(0, TestFeatureStateData.ConfigInt(KeyNothing));

    [Fact]
    public void FeatureStateWithConfigIntMissing9()
        => Equal(9, TestFeatureStateData.ConfigInt(KeyNothing, 9));



    [Fact]
    public void FeatureStateWithConfigString()
        => Equal("message", TestFeatureStateData.ConfigString(KeyWithString));

    [Fact]
    public void FeatureStateWithConfigMessageDefaultOk()
        => Equal("message", TestFeatureStateData.ConfigString(KeyWithString, "ok"));

    [Fact]
    public void FeatureStateWithConfigStringMissing()
        => Null(TestFeatureStateData.ConfigString(KeyNothing));

    [Fact]
    public void FeatureStateWithConfigStringMissingEmpty()
        => Equal("empty", TestFeatureStateData.ConfigString(KeyNothing, "empty"));


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
}