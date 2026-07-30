using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;


/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerification(TestDataGenerator generator) 
    : ToModelNameVerificationTests(generator, useInternal: false);

/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerificationInternal(TestDataGenerator generator)
    : ToModelNameVerificationTests(generator, useInternal: true);

/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelNameVerificationTests(TestDataGenerator generator, bool useInternal) : ToModelTestsBase(generator, useInternal)
{
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
