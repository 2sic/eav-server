using ToSic.Lib.Services;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

/// <summary>
/// Example service which uses options, but if not set, will use its own defaults.
/// </summary>
public class MockServiceSpawnNewCustomOptions(Generator<MockServiceSpawnNewCustomOptions, MockServiceOptions> selfGenerator)
    : ServiceWithSetup<MockServiceOptions>("Tst"), IServiceRespawn<MockServiceSpawnNewCustomOptions, MockServiceOptions>
{
    internal const string DefaultName = "custom";
    internal const int DefaultNumber = -1;

    protected override MockServiceOptions GetDefaultOptions() => new()
    {
        Name = DefaultName,
        Number = DefaultNumber
    };

    public MockServiceSpawnNewCustomOptions SpawnNew(MockServiceOptions? options = default)
        => selfGenerator.New(options ?? GetDefaultOptions());
}