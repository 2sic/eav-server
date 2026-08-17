namespace ToSic.Mock.LifetimeServices;

internal class MockScopedRequiringScoped(MockScopedStandalone scopedStandalone)
{
    public const int InitialValue = 1;
    public int Value { get; set; } = InitialValue;

    public MockScopedStandalone ScopedStandalone { get; } = scopedStandalone;
}