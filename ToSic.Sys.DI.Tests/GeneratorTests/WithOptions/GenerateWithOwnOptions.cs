namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class GenerateWithOwnOptions(MockServiceSpawnNewCustomOptions initialSvc)
{
    [Fact]
    public void InitialHasDefaultOptions()
    {
        Equal(MockServiceSpawnNewCustomOptions.DefaultName, initialSvc.MyOptions.Name);
        Equal(MockServiceSpawnNewCustomOptions.DefaultNumber, initialSvc.MyOptions.Number);
    }

    [Fact]
    public void GeneratesNewInstance()
        => NotEqual(initialSvc, initialSvc.SpawnNew(new()));

    [Fact]
    public void GeneratesWithOptions()
    {
        var obj = initialSvc.SpawnNew(new() { Name = "Test", Number = 42 });
        Equal("Test", obj.MyOptions.Name);
        Equal(42, obj.MyOptions.Number);
    }
}