namespace ToSic.Mock.LifetimeServices;

internal class MockTransientStandalone : IMockTransientStandalone
{
    public const int InitialValue = 99262;
    public virtual int Value { get; set; } = InitialValue;
}