using ToSic.Sys.DI;

namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class GenerateWithDefaultOptionsLightWip(MockServiceWithSetup initialSvc, Generator<MockServiceWithSetup, MockServiceOptions> generator)
{
    [Fact]
    public void InitialHasDefaultOptions()
    {
        Equal(MockServiceOptions.NameUndefined, initialSvc.MyOptions.Name);
        Equal(MockServiceOptions.NumberUndefined, initialSvc.MyOptions.Number);
    }

    [Fact]
    public void GeneratesNewInstance()
        => NotEqual(initialSvc, generator.New(new()));

    [Fact]
    public void GeneratesWithOptions()
    {
        var obj = generator.New(new() { Name = "Test", Number = 42 });
        Equal("Test", obj.MyOptions.Name);
        Equal(42, obj.MyOptions.Number);
    }
}