namespace ToSic.Mock.LifetimeServices;

internal class MockScopedStandalone
{
    public const int InitialValue = 59302;
    public virtual int Value { get; set; } = InitialValue;
}