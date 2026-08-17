namespace ToSic.Mock.LifetimeServices;

internal class MockScopedRequiringTransient(IMockTransientStandalone transientStandalone)
{
    public const int InitialValue = 1;
    public int Value { get; set; } = InitialValue;

    public IMockTransientStandalone TransientStandalone { get; } = transientStandalone;
}