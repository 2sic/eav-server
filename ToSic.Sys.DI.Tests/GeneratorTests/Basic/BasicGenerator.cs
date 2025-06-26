namespace ToSic.Lib.DI.GeneratorTests.Basic;

public class BasicGenerator
{
    private readonly Generator<TestObjectToGenerate> _generator;
    private readonly Generator<TestObjectToGenerate> _generatorWithInit;

    public BasicGenerator(Generator<TestObjectToGenerate> generator, Generator<TestObjectToGenerate> generateWithInit)
    {
        _generator = generator;
        _generatorWithInit = generateWithInit.SetInit(obj => obj.ToggleToInit = true);
    }

    [Fact]
    public void Generates1()
        => IsType<TestObjectToGenerate>(_generator.New());


    [Fact]
    public void Generates2()
    {
        var obj1 = _generator.New();
        var obj2 = _generator.New();
        NotSame(obj1, obj2);
    }

    [Fact]
    public void NoInitIsUnchanged()
        => False(_generator.New().ToggleToInit);

    [Fact]
    public void InitIsSet()
        => True(_generatorWithInit.New().ToggleToInit);
}