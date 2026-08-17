namespace ToSic.Mock.LifetimeServices;

internal class MockTransientRequiringScoped(MockScopedStandalone scopedStandalone)
{
    public const int InitialValue = 50362;
    public int Value { get; set; } = InitialValue;

    public MockScopedStandalone ScopedStandalone { get; } = scopedStandalone;
}