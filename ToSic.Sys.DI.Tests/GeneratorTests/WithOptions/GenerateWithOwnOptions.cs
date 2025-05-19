namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class GenerateWithOwnOptions(MockServiceSpawnNewCustomOptions initialSvc)
{
    [Fact]
    public void InitialHasDefaultOptions()
    {
        Equal(MockServiceSpawnNewCustomOptions.DefaultName, initialSvc.Options.Name);
        Equal(MockServiceSpawnNewCustomOptions.DefaultNumber, initialSvc.Options.Number);
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