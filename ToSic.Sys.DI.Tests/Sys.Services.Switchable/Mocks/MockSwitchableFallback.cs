namespace ToSic.Sys.Services.Switchable.Mocks;

public class MockSwitchableFallback: IMockSwitchableService
{
    internal const string Name = "FallbackSvc";

    public string NameId => Name;

    public bool IsViable() => true;

    public int Priority => 0;
}