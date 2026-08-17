namespace ToSic.Mock.LifetimeServices;

internal class MockScopedToReRegisterReqITransient(IMockTransientStandalone transientStandalone)
{
    public const int InitialValue = 602305;
    public virtual int Value { get; set; } = InitialValue;
    
    public IMockTransientStandalone TransientStandalone { get; } = transientStandalone;
}