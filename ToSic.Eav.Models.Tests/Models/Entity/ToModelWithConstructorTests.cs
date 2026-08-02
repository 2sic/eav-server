using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Same Tests - but for the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelWithConstructor(MockDataGenerator generator)
    : ToModelWithConstructorTests(generator, new ToModelTacPublic());

/// <summary>
/// Override for the ToModelInternal() test
/// </summary>
public class ToModelWithConstructorInternal(MockDataGenerator generator)
    : ToModelWithConstructorTests(generator, new ToModelTacInternal());

/// <summary>
/// Model With Constructor - must throw
/// </summary>
public abstract class ToModelWithConstructorTests(MockDataGenerator generator, IToModelTac toModelTac) : ToModelTestsBase(generator, toModelTac)
{

    #region Setup

    private interface IWithConstructor : IModelFromEntity<WithConstructor>;
    
    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record WithConstructor(string Something) : IModelFromEntity;

    #endregion
    
    
    [Fact]
    public void WithConstructorFromModel_Throws()
        => Throws<MissingMethodException>(GetModelSkipTypeCheck<WithConstructor>);

    [Fact]
    public void WithConstructorFromInterface_Throws()
        => Throws<MissingMethodException>(GetModelSkipTypeCheck<IWithConstructor>);

}
