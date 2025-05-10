using ToSic.Lib.Services;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

/// <summary>
/// Example service which uses options, but if not set, will use its own defaults.
/// </summary>
public class ServiceWithOwnOptions(Generator<ServiceWithOwnOptions> selfGenerator)
    : ServiceWithOptionsBase<ServiceWithOwnOptions, ServiceOptions>("Tst", selfGenerator)
{
    internal const string DefaultName = "custom";
    internal const int DefaultNumber = -1;

    protected override ServiceOptions GetDefaultOptions() => new()
    {
        Name = DefaultName,
        Number = DefaultNumber
    };
}