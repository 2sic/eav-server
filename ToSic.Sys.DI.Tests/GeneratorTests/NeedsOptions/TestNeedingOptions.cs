namespace ToSic.Lib.DI.GeneratorTests.NeedsOptions;

public class TestNeedingOptions: INeedsOptions<TestOptions>
{
    public TestOptions Options { get; set; }
}