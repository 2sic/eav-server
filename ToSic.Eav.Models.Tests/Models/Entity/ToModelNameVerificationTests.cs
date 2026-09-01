using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;


/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerification(MockDataGenerator generator) 
    : ToModelNameVerificationTests(generator, false);

/// <summary>
/// Test the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelNameVerificationInternal(MockDataGenerator generator)
    : ToModelNameVerificationTests(generator, true)
{
    public class Startup : ToSic.Eav.Models.Startup
    {
        public override void ConfigureServices(IServiceCollection services)
            => base.ConfigureServices(services.AddTransient<IToModelTac, ToModelTacInternal>());
    }
}

/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelNameVerificationTests(MockDataGenerator generator, bool useInternal)
{
    [Fact]
    public void VerifyCorrectToModelImplementation()
        => generator.VerifyCorrectToModelImplementation(useInternal);

    #region Name Checks


    [Fact]
    public void ModelWithNameMismatch_Throws()
        => Throws<KeyNotFoundException>(generator.GetModel<MockMetadataModelWrongName>);

    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameWrong_Throws()
        => Throws<KeyNotFoundException>(generator.GetModel<MockMetadataModelWithSpecsNameWrong>);
    
    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameRight_Works()
        => NotNull(generator.GetModel<MockMetadataModelWithSpecsNameRight>());

    [Fact]
    public void ModelWithNameMismatch_HasContentTypeAttribute_Works()
        => NotNull(generator.GetModel<MockMetadataModelWithContentTypeSpecsName>());

    [Fact]
    public void ModelWithNameMismatch_HasSpecsWithNameAsterisks_Works()
        => NotNull(generator.GetModel<MockMetadataModelWithSpecsNameAsterisks>());

    [Fact]
    public void ModelWithNameMismatch_SkipTypeCheck_Works()
        => NotNull(generator.GetModelSkipTypeCheck<MockMetadataModelWrongName>());

    [Fact]
    public void ModelWithNameMismatch_SkipTypeCheck_PropertyMatchesExpectedValue()
        => Equal((int)TargetTypes.Entity, generator.GetModelSkipTypeCheck<MockMetadataModelWrongName>()!.TargetType);

    #endregion
}
