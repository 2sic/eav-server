namespace ToSic.Mock.LifetimeServices;

/// <summary>
/// This will only be instantiated in the child scope through the interface, so it won't be discovered in the parent scope.
/// </summary>
internal class MockChildScopeOnlyTransientBasic : MockTransientStandalone
{
    public new const int InitialValue = 2603;
    public override int Value { get; set; } = InitialValue;
}