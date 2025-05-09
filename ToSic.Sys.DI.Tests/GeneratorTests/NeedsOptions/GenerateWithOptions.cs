namespace ToSic.Lib.DI.GeneratorTests.NeedsOptions;

public class GenerateWithOptions(IGenerator<TestNeedingOptions, TestOptions> generator)
{
    [Fact]
    public void GeneratesWithOptions()
    {
        var options = new TestOptions("Test", 42);
        var obj = generator.New(options);
        Equal("Test", obj.Options.Name);
        Equal(42, obj.Options.Number);
    }
}