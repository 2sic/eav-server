namespace ToSic.Eav.Core.Tests.PlumbingTests.DITests;

public class TestSwitchableSkip: ITestSwitchableService
{
    public string NameId => "Skip This";

    public bool IsViable() => false;

    public int Priority => 100;
}