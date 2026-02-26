namespace ToSic.Lib.DI.GeneratorTests.WithOptions;

public class GenerateWithDefaultOptions(MockServiceSpawnNewDefaultOptions initialSvc)
{
    [Fact]
    public void InitialHasDefaultOptions()
    {
        Equal(MockServiceOptions.NameUndefined, initialSvc.MyOptions.Name);
        Equal(MockServiceOptions.NumberUndefined, initialSvc.MyOptions.Number);
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