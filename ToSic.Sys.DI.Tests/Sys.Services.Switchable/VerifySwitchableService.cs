using ToSic.Sys.DI;
using ToSic.Sys.Services.Switchable.Mocks;

namespace ToSic.Sys.Services.Switchable;

public class VerifySwitchableService(ServiceSwitcher<IMockSwitchableService> switcher)
{
    public class Startup() : QuickStartup(s => s.AddMockSwitchableAndCoreServices());

    [Fact]
    public void FindKeepService() =>
        Equal(MockSwitchableKeep.Name, switcher.Value.NameId);

    [Fact]
    public void Has3Services() =>
        Equal(3, switcher.AllServices.Count);

    [Fact]
    public void NotCreateBeforeButCreatedAfter()
    {
        False(switcher.IsValueCreated, "shouldn't be created at first");
        var x = switcher.Value;
        True(switcher.IsValueCreated, "should be created afterwards");
    }

    [Fact]
    public void FindFallbackByName() =>
        Equal(MockSwitchableFallback.Name, switcher.ByNameId(MockSwitchableFallback.Name)?.NameId);
}