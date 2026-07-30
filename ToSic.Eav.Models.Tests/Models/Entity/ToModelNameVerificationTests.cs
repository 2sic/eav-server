using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;


/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerification(MockDataGenerator generator) 
    : ToModelNameVerificationTests(generator, useInternal: false);

/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerificationInternal(MockDataGenerator generator)
    : ToModelNameVerificationTests(generator, useInternal: true);

/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelNameVerificationTests(MockDataGenerator generator, bool useInternal) : ToModelTestsBase(generator, useInternal)
{
    #region Name Checks


    [Fact]
    public void ModelWithNameMismatch_Throws()
        => Throws<InvalidCastException>(GetModelNoParams<MockMetadataModelWrongName>);

    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameWrong_Throws()
        => Throws<InvalidCastException>(GetModelNoParams<MockMetadataModelWithSpecsNameWrong>);
    
    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameRight_Works()
        => NotNull(GetModelNoParams<MockMetadataModelWithSpecsNameRight>());

    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameAsterisks_Works()
        => NotNull(GetModelNoParams<MockMetadataModelWithSpecsNameAsterisks>());

    [Fact]
    public void ModelWithNameMismatch_SkipTypeCheck_Works()
        => NotNull(GetModelSkipTypeCheck<MockMetadataModelWrongName>());

    [Fact]
    public void ModelWithNameMismatch_SkipTypeCheck_PropertyMatchesExpectedValue()
        => Equal((int)TargetTypes.Entity, GetModelSkipTypeCheck<MockMetadataModelWrongName>()!.TargetType);

    #endregion
}
