namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class GenerateWithDefaultOptionsLightWip(TestServiceWithDefaultOptionsLightWip initialSvc, Generator<TestServiceWithDefaultOptionsLightWip, ServiceOptions> generator)
{
    [Fact]
    public void InitialHasDefaultOptions()
    {
        Equal(ServiceOptions.NameUndefined, initialSvc.Options.Name);
        Equal(ServiceOptions.NumberUndefined, initialSvc.Options.Number);
    }

    [Fact]
    public void GeneratesNewInstance()
        => NotEqual(initialSvc, generator.New(new()));

    [Fact]
    public void GeneratesWithOptions()
    {
        var obj = generator.New(new() { Name = "Test", Number = 42 });
        Equal("Test", obj.Options.Name);
        Equal(42, obj.Options.Number);
    }
}