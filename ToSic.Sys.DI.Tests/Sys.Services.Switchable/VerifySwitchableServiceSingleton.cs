using ToSic.Sys.DI;
using ToSic.Sys.Services.Switchable.Mocks;
using Xunit.Priority;

namespace ToSic.Sys.Services.Switchable;

/// <summary>
/// Important: the methods are called in A-Z order, so they must preserve the names with the number to achieve this
/// </summary>
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class VerifySwitchableServiceSingleton(
    ServiceSwitcherSingleton<IMockSwitchableService> switcher1,
    ServiceSwitcherSingleton<IMockSwitchableService> switcher2,
    ServiceSwitcherSingleton<IMockSwitchableService> switcher3,
    ServiceSwitcherSingleton<IMockSwitchableService> switcher4
)
{
    [Fact, Priority(1)]
    public void AccessSingletonN001() =>
        False(switcher1.IsValueCreated, "shouldn't be created at first");

    [Fact, Priority(2)]
    public void AccessSingletonN002()
    {
        False(switcher2.IsValueCreated, "shouldn't be created at first");
        var x = switcher2.Value;
        True(switcher2.IsValueCreated, "should be created afterwards");
    }

    [Fact, Priority(3)]
    public void AccessSingletonN003() =>
        True(switcher3.IsValueCreated, "should be created by now");

    [Fact, Priority(4)]
    public void FindFallbackByName() =>
        Equal(MockSwitchableFallback.Name, switcher4.ByNameId(MockSwitchableFallback.Name)?.NameId);
}

//[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
//class TestPriorityAttribute(int priority) : Attribute
//{
//    public int Priority { get; } = priority;
//}