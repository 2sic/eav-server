using ToSic.Lib.Services;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

/// <summary>
/// Example service which uses options, and is happy with the defaults if not set.
/// </summary>
public class MockServiceSpawnNewDefaultOptions(
    Generator<MockServiceSpawnNewDefaultOptions, MockServiceOptions> selfGenerator)
    : ServiceWithSetup<MockServiceOptions>("Tst"), IServiceRespawn<MockServiceSpawnNewDefaultOptions, MockServiceOptions>
{
    public MockServiceSpawnNewDefaultOptions SpawnNew(MockServiceOptions options)
        => selfGenerator.New(options);
}