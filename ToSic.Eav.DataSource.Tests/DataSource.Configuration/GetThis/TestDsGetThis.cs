namespace ToSic.Eav.DataSource.Configuration;

public class TestDsGetThis(DataSourceBase.MyServices services) : TestDataSourceBase(services)
{
    public const string ExpectedGetThisString = "ok";

    [Configuration(Fallback = ExpectedGetThisString)]
    public string GetThisString => Configuration.GetThis();

    [Configuration(Fallback = true)]
    public bool GetThisTrue => Configuration.GetThis(false); // should return true because it was configured

    [Configuration]
    public bool GetThisFalseDefault => Configuration.GetThis(false);
    [Configuration(Fallback = false)]
    public bool GetThisFalseInitialized => Configuration.GetThis(false);
}