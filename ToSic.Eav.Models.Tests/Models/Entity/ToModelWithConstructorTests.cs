using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Same Tests - but for the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelWithConstructor(MockDataGenerator generator)
    : ToModelWithConstructorTests(generator, useInternal: false);

/// <summary>
/// Override for the ToModelInternal() test
/// </summary>
public class ToModelWithConstructorInternal(MockDataGenerator generator)
    : ToModelWithConstructorTests(generator, useInternal: true);

/// <summary>
/// Model With Constructor - must throw
/// </summary>
public abstract class ToModelWithConstructorTests(MockDataGenerator generator, bool useInternal) : ToModelTestsBase(generator, useInternal)
{

    #region Setup

    private interface IWithConstructor : IModelFromEntity<WithConstructor>;
    
    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record WithConstructor(string Something) : IModelFromEntity;

    #endregion
    
    
    [Fact]
    public void WithConstructorFromModel_Throws()
        => Throws<InvalidCastException>(GetModelSkipTypeCheck<WithConstructor>);

    [Fact]
    public void WithConstructorFromInterface_Throws()
        => Throws<InvalidCastException>(GetModelSkipTypeCheck<IWithConstructor>);

}
