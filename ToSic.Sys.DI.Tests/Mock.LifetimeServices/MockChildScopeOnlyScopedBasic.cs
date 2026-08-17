namespace ToSic.Mock.LifetimeServices;

internal class MockChildScopeOnlyScopedBasic : MockScopedStandalone
{
    public new const int InitialValue = 20395;
    public override int Value { get; set; } = InitialValue;
}