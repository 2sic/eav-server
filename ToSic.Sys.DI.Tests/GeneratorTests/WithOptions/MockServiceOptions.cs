using System.Configuration;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public record MockServiceOptions
{
    public const string NameUndefined = "undefined";
    public const int NumberUndefined = -47;

    public string Name { get; init; } = NameUndefined;
    public int Number { get; init; } = NumberUndefined;
}