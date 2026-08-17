namespace ToSic.Mock.LifetimeServices;

internal class MockScopedStandaloneToReRegister
{
    public const int InitialValue = 602305;
    public virtual int Value { get; set; } = InitialValue;
}