using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Same Tests - but for the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModel(MockDataGenerator generator)
    : ToModelTests(generator, useInternal: false);

/// <summary>
/// Override for the ToModelInternal() test
/// </summary>
public class ToModelInternal(MockDataGenerator generator)
    : ToModelTests(generator, useInternal: true);

/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelTests(MockDataGenerator generator, bool useInternal) : ToModelTestsBase(generator, useInternal)
{

    #region Basic Direct Conversion (with Model Object, not Interface)

    [Fact]
    public void FromModelObject_NotNull()
    {
        var model = GetModelNoParams<MockMetadataModel>();
        NotNull(model);
    }
    
    [Fact]
    public void FromModelObject_PropertyTargetTypeMatches()
    {
        var model = GetModelNoParams<MockMetadataModel>()!;
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    #endregion


    #region Conversion from Interface which should just work

    [Fact]
    public void FromInterface_NotNull()
    {
        var model = GetModelSkipTypeCheck<IMockMetadataModel>();
        NotNull(model);
    }
    
    [Fact]
    public void FromInterface_ResultIsExpectedModelType()
    {
        var model = GetModelSkipTypeCheck<IMockMetadataModel>();
        IsType<MockMetadataModel>(model);
    }

    [Fact]
    public void FromInterface_PropertyTargetTypeMatches()
    {
        var model = GetModelNoParams<IMockMetadataModel>()!;
        Equal((int)TargetTypes.Entity, model.TargetType);
    }

    #endregion

    //#region Conversion from Interfaces which should not work

    //[Fact]
    //public void FromInterfaceDerived_Throws()
    //{
    //    var model = GetModelSkipTypeCheck<IMockMetadataModelDerived>();
    //    NotNull(model);
    //}
    
    //[Fact]
    //public void FromInterfaceDerivedReApplyingInterfaces_NotNull()
    //{
    //    var model = GetModelSkipTypeCheck<IMockMetadataModelDerivedReApplyingInterface>();
    //    NotNull(model);
    //}
    //#endregion

}
