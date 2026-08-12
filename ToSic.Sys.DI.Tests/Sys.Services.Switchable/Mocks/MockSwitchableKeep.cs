namespace ToSic.Sys.Services.Switchable.Mocks;

public class MockSwitchableKeep: IMockSwitchableService
{
    internal const string Name = "Keep";

    public string NameId => Name;

    public bool IsViable() => true;

    public int Priority => 10;
}