namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class GenerateWithDefaultOptions(ServiceWithDefaultOptions initialSvc)
{
    [Fact]
    public void InitialHasDefaultOptions()
    {
        Equal("undefined", initialSvc.Options.Name);
        Equal(0, initialSvc.Options.Number);
    }

    [Fact]
    public void GeneratesNewInstance()
        => NotEqual(initialSvc, initialSvc.SpawnNew(new()));

    [Fact]
    public void GeneratesWithOptions()
    {
        var obj = initialSvc.SpawnNew(new() { Name = "Test", Number = 42 });
        Equal("Test", obj.Options.Name);
        Equal(42, obj.Options.Number);
    }
}