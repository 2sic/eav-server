namespace ToSic.Sys.Services.Switchable.Mocks;

public class MockSwitchableSkip: IMockSwitchableService
{
    public string NameId => "Skip This";

    public bool IsViable() => false;

    public int Priority => 100;
}