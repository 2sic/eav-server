using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Same Tests - but for the internal ToModelInternal()
/// </summary>
/// <param name="generator"></param>
public class ToModelInheritance(MockDataGenerator<MockForInherit> generator)
    : ToModelInheritanceTests(generator, useInternal: false);

/// <summary>
/// Override for the ToModelInternal() test
/// </summary>
public class ToModelInheritanceInternal(MockDataGenerator<MockForInherit> generator)
    : ToModelInheritanceTests(generator, useInternal: true);

/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelInheritanceTests(MockDataGenerator<MockForInherit> generator, bool useInternal) : ToModelTestsBase(generator, useInternal)
{

    public record TestData(string? Value, string Name);
    
    public static TheoryData<TestData> OkNamesIfSetupOk =>
    [
        new(null, "null, do checks, but should work because underlying names are ok"),
        new("*", "* - disable checks"),
        new(nameof(MockForInherit), "exact correct name")
    ];
    public static TheoryData<TestData> ThrowingNamesIfSetupOk =>
    [
        new("", "empty - will check and return bad"),
        new("WRONG NAME", "incorrect name - will check and return bad"),
        new(nameof(IMockForInherit), "incorrect name - will check and return bad")
    ];
    public static TheoryData<TestData> ThrowingNamesIfSetupWrong =>
    [
        new(null, "null, do check, but fail because underlying names are wrong"),
        new("", "empty - will check and return bad"),
        new("WRONG NAME", "incorrect name - will check and return bad"),
        new(nameof(IMockForInherit), "incorrect name - will check and return bad")
    ];


    #region Basic Direct Conversion (with Model Object, not Interface)

    [Theory, MemberData(nameof(OkNamesIfSetupOk))] 
    public void FromModel_Class_NotNull(TestData typeNameCheck)
        => NotNull(GetModel<MockForInherit>(typeNameCheck.Value));

    [Theory, MemberData(nameof(ThrowingNamesIfSetupOk))]
    public void FromModel_Class_BadNames_Throws(TestData typeNameCheck)
        => Throws<InvalidCastException>(() => GetModel<MockForInherit>(typeNameCheck.Value));

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_Class_IsExpectedModelType(TestData typeNameCheck)
        => IsType<MockForInherit>(GetModel<MockForInherit>(typeNameCheck.Value));
    
    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_Class_PropertyTargetTypeMatches(TestData typeNameCheck)
        => Equal((int)TargetTypes.Entity, GetModel<MockForInherit>(typeNameCheck.Value).TargetType);

    #endregion

    
    #region Conversion of Derived Class, Basic. No Specs, no new interfaces. Should Throw because name is incorrect

    [Theory, MemberData(nameof(ThrowingNamesIfSetupWrong))]
    public void FromModel_Derived_Throws(TestData typeNameCheck)
        => Throws<InvalidCastException>(() => GetModel<MockForInheritDerivedBasic>(typeNameCheck.Value));

    #endregion


    #region Conversion of Derived Class, with specs having correct name

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_DerivedSpecsGood_NotNull(TestData typeNameCheck)
        => NotNull(GetModel<MockForInheritDerivedSpecsGood>(typeNameCheck.Value));

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_DerivedSpecsGood_IsExpectedModelType(TestData typeNameCheck)
        => IsType<MockForInheritDerivedSpecsGood>(GetModel<MockForInheritDerivedSpecsGood>(typeNameCheck.Value));

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_DerivedSpecsGood_PropertyTargetTypeMatches(TestData typeNameCheck)
        => Equal((int)TargetTypes.Entity, GetModel<MockForInheritDerivedSpecsGood>(typeNameCheck.Value).TargetType);

    #endregion


    #region Conversion of Derived Class, with specs bad (incorrect name)

    [Fact]
    public void FromModel_DerivedSpecsBad_Throws()
        => Throws<InvalidCastException>(GetModel<MockForInheritDerivedSpecsBad>);

    [Fact]
    public void FromModel_DerivedSpecsBad_IgnoreName_NotNull()
        => NotNull(GetModelSkipTypeCheck<MockForInheritDerivedSpecsBad>());

    #endregion


    #region Conversion of Derived Class, with specs Asterisks (skip name check)

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_DerivedSpecsAsterisks_NotNull(TestData typeNameCheck)
        => NotNull(GetModel<MockForInheritDerivedSpecsAsterisks>(typeNameCheck.Value));

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_DerivedSpecsAsterisks_IsExpectedModelType(TestData typeNameCheck)
        => IsType<MockForInheritDerivedSpecsAsterisks>(GetModel<MockForInheritDerivedSpecsAsterisks>(typeNameCheck.Value));

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromModel_DerivedSpecsAsterisks_PropertyTargetTypeMatches(TestData typeNameCheck)
        => Equal((int)TargetTypes.Entity, GetModel<MockForInheritDerivedSpecsAsterisks>(typeNameCheck.Value).TargetType);

    #endregion


    #region Conversion from Interface which should just work

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromInterface_NotNull(TestData typeNameCheck)
        => NotNull(GetModel<IMockForInherit>(typeNameCheck.Value));

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromInterface_IsExpectedModelType(TestData typeNameCheck)
        => IsType<MockForInherit>(GetModel<IMockForInherit>(typeNameCheck.Value));

    [Theory, MemberData(nameof(OkNamesIfSetupOk))]
    public void FromInterface_PropertyTargetTypeMatches(TestData typeNameCheck)
        => Equal((int)TargetTypes.Entity, GetModel<IMockForInherit>(typeNameCheck.Value).TargetType);

    #endregion

    
    #region Conversion from Interfaces which should not work

    [Fact]
    public void FromInterface_DerivedButNotImplemented_Throws_UnderlyingModelDoesNotImplementInterface()
        => Throws<InvalidCastException>(GetModelSkipTypeCheck<IMockForInherit_NotImplemented>);

    [Fact]
    public void FromInterface_DerivedAndImplemented_NotNull_ButExtremelyUnlikely()
        => NotNull(GetModelSkipTypeCheck<IMockForInherit_Implemented>());

    
    
    [Fact]
    public void FromInterface_DerivedReApplyingInterfaces_NotNull()
        => NotNull(GetModelSkipTypeCheck<IMockForInherit_ReApplyingInterface>());


    [Fact]
    public void FromInterface_DerivedReApplyingInterfaceForInterface_NotNull()
        => Throws<InvalidCastException>(GetModelSkipTypeCheck<IMockForInherit_ReApplyingInterfaceForInterface>);

    [Fact]
    public void FromInterface_DerivedReApplyingInterfaceWithSpecsGood_NotNull()
        => NotNull(GetModelSkipTypeCheck<IMockForInherit_ReApplyingInterfaceWithSpecsGood>());

    [Fact]
    public void FromInterface_DerivedReApplyingInterfaceWithSpecsBad_NotNull()
        => Throws<InvalidCastException>(GetModelSkipTypeCheck<IMockForInherit_ReApplyingInterfaceWithSpecsBad>);
    
    [Fact]
    public void FromInterface_DerivedReApplyingInterfaceWithSpecsAsterisks_NotNull()
        => NotNull(GetModelSkipTypeCheck<IMockForInherit_ReApplyingInterfaceWithSpecsAsterisks>());

    #endregion

}
