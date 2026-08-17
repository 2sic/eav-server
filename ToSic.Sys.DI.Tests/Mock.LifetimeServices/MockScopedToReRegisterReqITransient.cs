namespace ToSic.Mock.LifetimeServices;

internal interface IMockScopedToReRegisterReqITransient
{
    int Value { get; set; }
    IMockTransientStandalone TransientStandalone { get; }
}

internal class MockScopedToReRegisterReqITransient(IMockTransientStandalone transientStandalone) : IMockScopedToReRegisterReqITransient
{
    public const int InitialValue = 602305;
    public int Value { get; set; } = InitialValue;
    
    public IMockTransientStandalone TransientStandalone { get; } = transientStandalone;
}