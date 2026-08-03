using ToSic.Sys.Utils.Types;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Sys.Utils.TypeFactoryTests;

public class TypeFactoryTests
{
    private class ClassWithoutConstructor;

    // ReSharper disable once ClassNeverInstantiated.Local
    private class ClassWithConstructor(string something);

    //[Fact]
    //public void AaaAtFirstCacheIsEmpty()
    //{
    //    Empty(TypeFactory.TypeFactoryCache);
    //}

    [Fact]
    public void A_TypeFactoryCreateSimpleObject_NotNull()
    {
        var x = TypeFactory.CreateInstance(typeof(ClassWithoutConstructor));
        NotNull(x);
    }

    //[Fact]
    //public void A_TypeFactoryCreateSimpleObject_FirstOneInCache()
    //{
    //    var x = TypeFactory.CreateInstance(typeof(ClassWithoutConstructor));
    //    Single(TypeFactory.TypeFactoryCache);
    //}

    [Fact]
    public void A_TypeFactoryCreateSimpleObjectGeneric()
    {
        var x = TypeFactory.CreateInstance<ClassWithoutConstructor>();
        NotNull(x);
        //Single(TypeFactory.TypeFactoryCache);
    }

    [Fact]
    public void B_ThrowsIfNonEmptyConstructor() =>
        Throws<MissingConstructorException>(TypeFactory.CreateInstance<ClassWithConstructor>);

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
