using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Models.TestData;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Same Tests - but for the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelWithConstructor(MockDataGenerator generator)
    : ToModelWithConstructorTests(generator, false);

/// <summary>
/// Override for the ToModelInternal() test
/// </summary>
public class ToModelWithConstructorInternal(MockDataGenerator generator)
    : ToModelWithConstructorTests(generator, true)
{
    public class Startup : ToSic.Eav.Models.Startup
    {
        public override void ConfigureServices(IServiceCollection services)
            => base.ConfigureServices(services.AddTransient<IToModelTac, ToModelTacInternal>());
    }
}

/// <summary>
/// Model With Constructor - must throw
/// </summary>
public abstract class ToModelWithConstructorTests(MockDataGenerator generator, bool useInternal)
{
    [Fact]
    public void VerifyCorrectToModelImplementation()
        => generator.VerifyCorrectToModelImplementation(useInternal);

    #region Setup

    private interface IWithConstructor : IModelFromEntity<WithConstructor>;
    
    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record WithConstructor(string Something) : IModelFromEntity;

    #endregion
    
    
    [Fact]
    public void WithConstructorFromModel_Throws()
        => Throws<MissingConstructorException>(generator.GetModelSkipTypeCheck<WithConstructor>);

    [Fact]
    public void WithConstructorFromInterface_Throws()
        => Throws<MissingConstructorException>(generator.GetModelSkipTypeCheck<IWithConstructor>);

}
