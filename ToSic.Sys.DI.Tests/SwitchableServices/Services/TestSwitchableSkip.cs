namespace ToSic.Lib.DI.SwitchableServices.Services;

public class TestSwitchableSkip: ITestSwitchableService
{
    public string NameId => "Skip This";

    public bool IsViable() => false;

    public int Priority => 100;
}