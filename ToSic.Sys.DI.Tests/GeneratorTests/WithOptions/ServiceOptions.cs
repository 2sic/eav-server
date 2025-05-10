namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public record ServiceOptions
{
    public string Name { get; init; } = "undefined";
    public int Number { get; init; } = 0;
}