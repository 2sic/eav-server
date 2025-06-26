namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class GenerateWithDefaultOptions(MockServiceSpawnNewDefaultOptions initialSvc)
{
    [Fact]
    public void InitialHasDefaultOptions()
    {
        Equal(MockServiceOptions.NameUndefined, initialSvc.Options.Name);
        Equal(MockServiceOptions.NumberUndefined, initialSvc.Options.Number);
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