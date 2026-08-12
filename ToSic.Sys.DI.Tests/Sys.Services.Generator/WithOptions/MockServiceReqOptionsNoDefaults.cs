namespace ToSic.Sys.Services.Generator.WithOptions;

/// <summary>
/// Example service which uses options,
/// but it will not generate defaults, so if they are missing, it should throw.
/// </summary>
public class MockServiceReqOptionsNoDefaults()
    : ServiceWithSetup<MockServiceOptions>("Tst")
{
    // explicitly don't implement GetDefaultOptions
    //protected override MockServiceOptions GetDefaultOptions() => new();

    /// <summary>
    /// Access the options - if faulty, will throw.
    /// </summary>
    public string AccessOptions => MyOptions.Name;
}