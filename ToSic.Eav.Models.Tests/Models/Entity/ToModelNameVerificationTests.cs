using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Test the public ToModel()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerificationTests(TestDataGenerator generator) : ToModelNameVerificationTestsShared
{
    protected override TModel GetModelNoParams<TModel>()
        => generator.CreateMetadataForDecorator().ToModelTac<TModel>()!;
    
    protected override TModel GetModelSkipTypeCheck<TModel>()
        => generator.CreateMetadataForDecorator().ToModelTac<TModel>(options: ToModelOptions.DisableTypeNameCheck)!;
}

/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerificationInternal(TestDataGenerator generator) : ToModelNameVerificationTestsShared
{
    protected override TModel GetModelNoParams<TModel>()
        => generator.CreateMetadataForDecorator().ToModelInternalTac<TModel>(new())!;
    
    protected override TModel GetModelSkipTypeCheck<TModel>()
        => generator.CreateMetadataForDecorator().ToModelInternalTac<TModel>(ToModelOptions.DisableTypeNameCheck)!;
}


/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelNameVerificationTestsShared
{
    #region Test Setup Helpers to create models either using the internal ToModelInternal or the public ToModel

    protected abstract TModel? GetModelNoParams<TModel>()
        where TModel : class, IModelFromEntity;

    protected abstract TModel? GetModelSkipTypeCheck<TModel>()
        where TModel : class, IModelFromEntity;

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
