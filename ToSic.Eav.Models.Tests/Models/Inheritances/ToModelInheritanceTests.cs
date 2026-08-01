using ToSic.Eav.Metadata;
using ToSic.Eav.Models.Entity;
using ToSic.Eav.Models.TestData;
using Xunit.DependencyInjection;

// ReSharper disable UnusedMember.Global
#pragma warning disable CA1825
#pragma warning disable xUnit1045

namespace ToSic.Eav.Models.Inheritances;


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
public abstract class ToModelInheritanceTests(MockDataGenerator<MockForInherit> generator, bool useInternal)
    : ToModelTestsBase(generator, useInternal)
{
    #region Test cases which should all just work as expected - Tests from external Generator combining object creation and name combinations

    [Theory, MethodData(nameof(TestCaseGenerator.OkTypesAndNamesIfSetupOk), typeof(TestCaseGenerator))]
    public void FromVarious_NotNull(TestCaseTypeAndName testCase)
        => NotNull(testCase.GeneratedObject);

    [Theory, MethodData(nameof(TestCaseGenerator.OkTypesAndNamesIfSetupOk), typeof(TestCaseGenerator))]
    public void FromVarious_IsExpectedModelType(TestCaseTypeAndName testCase)
        => IsType(testCase.ExpectedType, testCase.GeneratedObject);
    
    [Theory, MethodData(nameof(TestCaseGenerator.OkTypesAndNamesIfSetupOk), typeof(TestCaseGenerator))]
    public void FromVarious_PropertyTargetTypeMatches(TestCaseTypeAndName testCase)
        => Equal((int)TargetTypes.Entity, ((MockForInherit)testCase.GeneratedObject).TargetType);

    #endregion



    #region Direct Conversion which are expected to fail

    [Theory, MethodData(nameof(TestCaseGenerator.ThrowingTypesAndNamesIfSetupOk), typeof(TestCaseGenerator))]
    public void FromVarious_SetupOkButBadNames_Throws(TestCaseTypeGenAndName testCase)
        => Throws<InvalidCastException>(() => testCase.Generator());


    #endregion

    
    
    #region Conversion of Derived Class, Basic. No Specs, no new interfaces. Should Throw because name is incorrect

    [Theory, MethodData(nameof(TestCaseGenerator.ThrowingTypesAndNamesIfSetupWrong), typeof(TestCaseGenerator))]
    public void FromVarious_SetupWrong_Throws(TestCaseTypeGenAndName testCase)
        => Throws<InvalidCastException>(() => testCase.Generator());

    #endregion

    

    #region Conversion of Derived Class, with specs bad (incorrect name)
    
    [Fact]
    public void FromModel_DerivedSpecsBad_IgnoreName_NotNull()
        => NotNull(GetModelSkipTypeCheck<MockForInheritDerivedSpecsBad>());

    #endregion

    
    #region Conversion from Interfaces which should not work

    [Fact]
    public void FromInterface_DerivedAndImplemented_NotNull_ButExtremelyUnlikely()
        => NotNull(GetModelSkipTypeCheck<IMockForInherit_Implemented>());

    
    
    [Fact]
    public void FromInterface_DerivedReApplyingInterfaces_NotNull()
        => NotNull(GetModelSkipTypeCheck<IMockForInherit_ReApplyingInterface>());

    [Fact]
    public void FromInterface_DerivedReApplyingInterfaceWithSpecsGood_NotNull()
        => NotNull(GetModelSkipTypeCheck<IMockForInherit_ReApplyingInterfaceWithSpecsGood>());

    #endregion

}
