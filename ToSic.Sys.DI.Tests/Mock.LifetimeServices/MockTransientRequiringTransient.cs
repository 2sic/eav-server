namespace ToSic.Mock.LifetimeServices;

internal class MockTransientRequiringTransient(IMockTransientStandalone transientStandalone)
{
    public const int InitialValue = 2963;
    public int Value { get; set; } = InitialValue;
    
    public IMockTransientStandalone TransientStandalone { get; } = transientStandalone;
}