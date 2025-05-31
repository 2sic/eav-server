using ToSic.Sys.Configuration;

namespace ToSic.Sys.Core.Tests.ConfigurationTests;

public class GlobalConfigurationTests
{
    private IGlobalConfiguration NewConfig => new GlobalConfiguration();

    [Fact]
    public void TestSetThisGetThis()
    {
        var config = NewConfig;
        config.SetThis("myValue");
        Equal("myValue", config.GetThis());
    }

    [Fact]
    public void GetErrorOnNullWhenOk()
    {
        var config = NewConfig;
        config.SetThis("myValue");
        Equal("myValue", config.GetThisErrorOnNull());
    }

    [Fact]
    public void GetErrorOnNullWhenNotSet()
        => Throws<ArgumentNullException>(() => NewConfig.GetThisErrorOnNull());

    [Fact]
    public void GetThisOrSet()
    {
        var config = NewConfig;
        // Verify initially null
        Null(config.GetThis());

        // Use GetThisOrSet to set a value
        Equal("myValue", config.GetThisOrSet(() => "my" + "Value"));

        // Verify the value is set
        Equal("myValue", config.GetThis());
    }
}