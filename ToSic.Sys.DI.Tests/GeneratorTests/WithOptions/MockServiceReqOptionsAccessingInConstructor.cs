namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

/// <summary>
/// Example of a bad service which tries to access the options during construction.
/// </summary>
public class MockServiceReqOptionsAccessingInConstructor
    : ServiceWithSetup<MockServiceOptions>
{
    public MockServiceReqOptionsAccessingInConstructor(): base("Tst")
    {
        // accessing options in constructor should throw, because they are not yet set
        // ReSharper disable once VirtualMemberCallInConstructor
        var name = MyOptions.Name;
    }
}