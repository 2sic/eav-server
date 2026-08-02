using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.TestData;
using ToSic.Sys.Utils.Types;
using Xunit.DependencyInjection;

// ReSharper disable UnusedMember.Global
#pragma warning disable CA1825
#pragma warning disable xUnit1045

namespace ToSic.Eav.Models.Inheritances;


/// <summary>
/// Same Tests - but for the public ToModel()
/// </summary>
public class ToModelInheritance(MockDataGenerator<MockForInherit> generator) : ToModelInheritanceTests(generator, false);

/// <summary>
/// Override for the ToModelInternal() test
/// </summary>
public class ToModelInheritanceInternal(MockDataGenerator<MockForInherit> generator) : ToModelInheritanceTests(generator, true)
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
            => base.ConfigureServices(services.AddTransient<IToModelTac, ToModelTacInternal>());
    }
}

/// <summary>
/// Shared tests for ToModel and ToModelInternal
/// </summary>
public abstract class ToModelInheritanceTests(MockDataGenerator<MockForInherit> generator, bool useInternal)
{
    #region Test Case Generator - Prepare for later Partials

    [Fact]
    public void VerifyCorrectToModelImplementation()
        => generator.VerifyCorrectToModelImplementation(useInternal);

    public partial class TestCaseGenerator(MockDataGenerator<MockForInherit> generator): TestCaseGeneratorBase(generator);

    #endregion

    #region Test cases which should all just work as expected - Tests from external Generator combining object creation and name combinations

    public partial class TestCaseGenerator
    {
        private static TestCaseName[] SkipNameCheck =>
        [
            new("*", "Skip type Check"),
        ];


        private static TestCaseName[] OkNamesIfSetupOk =>
        [
            new(null, "null, do checks, but should work because underlying names are ok"),
            new("*", "* - disable checks"),
            new(nameof(MockForInherit), "exact correct name")
        ];

        public IEnumerable<object[]> ValidTypesWithGoodNames()
            => CreateTestCases<TestCase_IsValidAttribute>(OkNamesIfSetupOk);
    }



    [Theory, MethodData(nameof(TestCaseGenerator.ValidTypesWithGoodNames), typeof(TestCaseGenerator))]
    public void Valid_NotNull(TestCaseTypeAndName testCase)
        => NotNull(testCase.Generator());

    [Theory, MethodData(nameof(TestCaseGenerator.ValidTypesWithGoodNames), typeof(TestCaseGenerator))]
    public void Valid_PropertyMatchesData(TestCaseTypeAndName testCase)
        => Equal((int)TargetTypes.Entity, ((MockForInherit)testCase.Generator()!).TargetType);

    #endregion

    
    
    #region Expected Type Tests

    public partial class TestCaseGenerator
    {
        public IEnumerable<object[]> ExpectedTypesWithGoodNames()
            => CreateTestCases<TestCase_ExpectedTypeAttribute>(SkipNameCheck);
    }
    
    [Theory, MethodData(nameof(TestCaseGenerator.ExpectedTypesWithGoodNames), typeof(TestCaseGenerator))]
    public void Valid_IsExpectedModelType(TestCaseTypeAndName testCase)
        => IsType(((TestCase_ExpectedTypeAttribute)testCase.Attribute).Type ?? testCase.OriginalType,
            testCase.Generator());

    #endregion

    

    #region Direct Conversion which are expected to fail

    public partial class TestCaseGenerator
    {
        private static TestCaseName[] BadNames =>
        [
            new("", "empty - will check and return bad"),
            new("WRONG NAME", "incorrect name - will check and return bad"),
            new(nameof(IMockForInherit), "incorrect name - will check and return bad")
        ];

        public IEnumerable<object[]> ValidTypesWithBadNames()
            => CreateTestCases<TestCase_IsValidAttribute>(BadNames);
    }

    [Theory, MethodData(nameof(TestCaseGenerator.ValidTypesWithBadNames), typeof(TestCaseGenerator))]
    public void Valid_WithBadNameParam_Throws(TestCaseTypeAndName testCase)
        => Throws<KeyNotFoundException>(() => testCase.Generator());


    #endregion



    #region Conversion of Derived Class, Basic. No Specs, no new interfaces. Should Throw because name is incorrect

    public partial class TestCaseGenerator
    {
        private static TestCaseName[] BadNamesInclAutoName =>
        [
            new(null, "null, do check, but fail because underlying names are wrong"),
            ..BadNames,
        ];

        public IEnumerable<object[]> TypesWithBadAutoNames()
            => CreateTestCases<TestCase_BadNameAttribute>(BadNamesInclAutoName);
    }

    [Theory, MethodData(nameof(TestCaseGenerator.TypesWithBadAutoNames), typeof(TestCaseGenerator))]
    public void BadAutoNames_Throws(TestCaseTypeAndName testCase)
        => Throws<KeyNotFoundException>(() => testCase.Generator());

    
    
    public partial class TestCaseGenerator
    {
        public IEnumerable<object[]> TypesWithBadAutoNamesSkipTypeCheck()
            => CreateTestCases<TestCase_BadNameAttribute>(SkipNameCheck);
    }


    [Theory, MethodData(nameof(TestCaseGenerator.TypesWithBadAutoNamesSkipTypeCheck), typeof(TestCaseGenerator))]
    public void BadAutoNames_WithIgnoreName_Works(TestCaseTypeAndName testCase)
        => NotNull(testCase.Generator());

    #endregion

    #region Cast Problems

    public partial class TestCaseGenerator
    {
        public IEnumerable<object[]> TypesWithBadInterfaceCast()
            => CreateTestCases<TestCase_BadInterfaceCastAttribute>(SkipNameCheck);
    }
    
    [Theory, MethodData(nameof(TestCaseGenerator.TypesWithBadInterfaceCast), typeof(TestCaseGenerator))]
    public void BadInterfaceCast_Throws(TestCaseTypeAndName testCase)
        => Throws<InvalidCastException>(() => testCase.Generator());


    #endregion

}
