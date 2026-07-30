using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Same Tests - but for the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModel(TestDataGenerator generator)
    : ToModelTests(generator, useInternal: false);

/// <summary>
/// Override for the ToModelInternal() test
/// </summary>
public class ToModelInternal(TestDataGenerator generator)
    : ToModelTests(generator, useInternal: true);

/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelTests(TestDataGenerator generator, bool useInternal) : ToModelTestsBase(generator, useInternal)
{

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

    [Fact]
    public void FromInterface_PropertyTargetTypeMatches()
    {
        var model = GetModelNoParams<IMockModelMetadataForDecorator>()!;
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    #endregion
    
}
