using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Test the public ToModel()
/// </summary>
/// <param name="generator"></param>
public class ToModel(TestDataGenerator generator) : ToModelTestsShared
{
    protected override TModel GetModelNoParams<TModel>()
        => generator.CreateMetadataForDecorator().ToModelTac<TModel>()!;
    
    protected override TModel GetModelSkipTypeCheck<TModel>()
        => generator.CreateMetadataForDecorator().ToModelTac<TModel>(options: new() { TypeNameCheck = ToModelOptions.ModelTypeCheck.Skip })!;
}

/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelInternal(TestDataGenerator generator) : ToModelTestsShared
{
    protected override TModel GetModelNoParams<TModel>()
        => generator.CreateMetadataForDecorator().ToModelInternalTac<TModel>(new())!;
    
    protected override TModel GetModelSkipTypeCheck<TModel>()
        => generator.CreateMetadataForDecorator().ToModelInternalTac<TModel>(new() { TypeNameCheck = ToModelOptions.ModelTypeCheck.Skip })!;
}


/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelTestsShared
{
    #region Test Setup Helpers to create models either using the internal ToModelInternal or the public ToModel

    protected abstract TModel? GetModelNoParams<TModel>()
        where TModel : class, IModelFromEntity;

    protected abstract TModel? GetModelSkipTypeCheck<TModel>()
        where TModel : class, IModelFromEntity;

    #endregion


    #region Basic Direct Conversion (with Model Object, not Interface)

    [Fact]
    public void FromModelObject_NotNull()
    {
        var model = GetModelNoParams<MockModelMetadataForDecorator>();
        NotNull(model);
    }
    
    [Fact]
    public void FromModelObject_PropertyTargetTypeMatches()
    {
        var model = GetModelNoParams<MockModelMetadataForDecorator>()!;
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    #endregion


    #region Conversion from Interface

    [Fact]
    public void FromInterface_NotNull()
    {
        var model = GetModelSkipTypeCheck<IMockModelMetadataForDecorator>();
        NotNull(model);
    }
    
    [Fact]
    public void FromInterface_ResultIsExpectedModelType()
    {
        var model = GetModelSkipTypeCheck<IMockModelMetadataForDecorator>();
        IsType<MockModelMetadataForDecorator>(model);
    }

    #endregion


    #region Model With Constructor - must throw

    private interface IWithConstructor : IModelFromEntity<WithConstructor>;
    
    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record WithConstructor(string Something) : IModelFromEntity;

    [Fact]
    public void WithConstructorFromModel_Throws()
        => Throws<InvalidCastException>(GetModelSkipTypeCheck<WithConstructor>);

    [Fact]
    public void WithConstructorFromInterface_Throws()
        => Throws<InvalidCastException>(GetModelSkipTypeCheck<IWithConstructor>);

    #endregion

    

    #region Name Checks


    [Fact]
    public void ModelWithNameMismatch_Throws()
        => Throws<InvalidCastException>(GetModelNoParams<MockModelMetadataForDecoratorWrongName>);

    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameWrong_Throws()
        => Throws<InvalidCastException>(GetModelNoParams<MockModelMetadataForDecoratorWithModelSpecsNameWrong>);
    
    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameRight_Works()
        => NotNull(GetModelNoParams<MockModelMetadataForDecoratorWithModelSpecsNameRight>());

    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameAsterisks_Works()
        => NotNull(GetModelNoParams<MockModelMetadataForDecoratorWithModelSpecsNameAsterisks>());

    [Fact]
    public void ModelWithNameMismatch_SkipTypeCheck_Works()
        => NotNull(GetModelSkipTypeCheck<MockModelMetadataForDecoratorWrongName>());

    [Fact]
    public void ModelWithNameMismatch_SkipTypeCheck_PropertyMatchesExpectedValue()
        => Equal((int)TargetTypes.Entity, GetModelSkipTypeCheck<MockModelMetadataForDecoratorWrongName>()!.TargetType);

    #endregion
}
