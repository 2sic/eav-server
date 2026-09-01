using ToSic.Sys.DI;

namespace ToSic.Sys.Services.Generator.WithOptions;

/// <summary>
/// Example service which uses options, and is happy with the defaults if not set.
/// </summary>
public class MockServiceSpawnNewDefaultOptions(
    Generator<MockServiceSpawnNewDefaultOptions, MockServiceOptions> selfGenerator)
    : ServiceWithSetup<MockServiceOptions>("Tst"), IServiceRespawn<MockServiceSpawnNewDefaultOptions, MockServiceOptions>
{
    protected override MockServiceOptions GetDefaultOptions() => new();
    
    public MockServiceSpawnNewDefaultOptions SpawnNew(MockServiceOptions options)
        => selfGenerator.New(options);

}