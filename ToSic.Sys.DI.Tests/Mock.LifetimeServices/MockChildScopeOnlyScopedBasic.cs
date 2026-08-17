namespace ToSic.Mock.LifetimeServices;

/// <summary>
/// This is the interface we must look for, so we can register it separately from the proper implementation.
/// </summary>
internal interface IMockChildScopeOnlyScopedBasic
{
    int Value { get; set; }
}

internal class MockChildScopeOnlyScopedBasic : MockScopedStandalone, IMockChildScopeOnlyScopedBasic
{
    public new const int InitialValue = 20395;
    public override int Value { get; set; } = InitialValue;
}

internal class MockChildScopeOnlyScopedBasicInitialThrows : MockChildScopeOnlyScopedBasic
{
    public MockChildScopeOnlyScopedBasicInitialThrows()
    {
        throw new NotSupportedException();
    }
}