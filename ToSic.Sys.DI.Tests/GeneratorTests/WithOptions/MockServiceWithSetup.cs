using ToSic.Lib.Services;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

/// <summary>
/// Example service which uses options, and is happy with the defaults if not set.
/// </summary>
public class MockServiceWithSetup()
    : ServiceWithSetup<ServiceOptions>("Tst");