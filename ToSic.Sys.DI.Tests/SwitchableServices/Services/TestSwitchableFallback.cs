namespace ToSic.Lib.DI.SwitchableServices.Services;

public class TestSwitchableFallback: ITestSwitchableService
{
    internal const string Name = "FallbackSvc";

    public string NameId => Name;

    public bool IsViable() => true;

    public int Priority => 0;
}