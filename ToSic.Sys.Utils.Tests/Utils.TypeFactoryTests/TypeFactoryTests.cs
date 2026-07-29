using ToSic.Sys.Utils.Types;

namespace ToSic.Sys.Utils.TypeFactoryTests;

public class TypeFactoryTests
{
    private class ClassWithoutConstructor { }

    private class ClassWithConstructor(string something) { }

    //[Fact]
    //public void AaaAtFirstCacheIsEmpty()
    //{
    //    Empty(TypeFactory.Cache);
    //}


    [Fact]
    public void TypeFactoryCreateSimpleObject()
    {
        var x = TypeFactory.CreateInstance(typeof(ClassWithoutConstructor));
        NotNull(x);
        Single(TypeFactory.TypeFactoryCache);
    }

    [Fact]
    public void TypeFactoryCreateSimpleObjectGeneric()
    {
        var x = TypeFactory.CreateInstance<ClassWithoutConstructor>();
        NotNull(x);
        Single(TypeFactory.TypeFactoryCache);
    }

    [Fact]
    public void ThrowsIfNonEmptyConstructor() =>
        Throws<InvalidOperationException>(() =>
        {
            TypeFactory.CreateInstance<ClassWithConstructor>();
        });

    //private class ClassToTestMultipleRuns { }

    //[Fact]
    //public void SecondRunsAreMuchFaster()
    //{
    //    var time = Stopwatch.StartNew();
    //    var first = TypeFactory.Create<ClassToTestMultipleRuns>();
    //    var firstTime = time.ElapsedTicks;

    //    time = Stopwatch.StartNew();
    //    var again = TypeFactory.Create<ClassToTestMultipleRuns>();
    //    var secondTime = time.ElapsedTicks;
        
    //    True(secondTime > firstTime);
    //}
}
