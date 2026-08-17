namespace ToSic.Mock.LifetimeServices;

/// <summary>
/// This will be added/pre-registered in the parent scope,
/// but without the interface; the interface use will happen in the child scope
/// </summary>
internal class MockChildScopeOnlyTransientPreRegistered : MockTransientStandalone
{
    public new const int InitialValue = 264903;
    public override int Value { get; set; } = InitialValue;
}