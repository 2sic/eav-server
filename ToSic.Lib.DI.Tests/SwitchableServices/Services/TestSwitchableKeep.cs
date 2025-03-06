namespace ToSic.Lib.DI.SwitchableServices.Services;

public class TestSwitchableKeep: ITestSwitchableService
{
    internal const string Name = "Keep";

    public string NameId => Name;

    public bool IsViable() => true;

    public int Priority => 10;
}