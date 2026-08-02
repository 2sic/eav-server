using ToSic.Eav.Models.Entity;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Inheritances;

/// <summary>
/// Simple test case information.
/// </summary>
/// <param name="Name">Name to apply when trying to get the model</param>
/// <param name="Notes">Additional notes about the test case</param>
public record TestCaseName(string? Name, string Notes);

/// <summary>
/// Test case information for a specific type and name, including a generator function and expected type.
/// This is used to ensure that any exceptions thrown happen during the test, not during data-generation.
/// </summary>
/// <param name="Name">Name to apply when trying to get the model</param>
/// <param name="Generator">Function to generate the object for the test case</param>
/// <param name="ExpectedType">The expected type of the generated object</param>
/// <param name="Notes">Additional notes about the test case</param>
public record TestCaseTypeAndName(string? Name, Func<object> Generator, Type ExpectedType, string Notes);

public class TestCaseGenerator(MockDataGenerator<MockForInherit> generator) : ToModelTestsBase(generator, false)
{
    private static TestCaseName[] OkNamesIfSetupOk =>
    [
        new(null, "null, do checks, but should work because underlying names are ok"),
        new("*", "* - disable checks"),
        new(nameof(MockForInherit), "exact correct name")
    ];

    public IEnumerable<object[]> OkTypesAndNamesBecauseSetupOk()
        => OkNamesIfSetupOk
            .SelectMany(testCase => new object[][] {
                [CreateTestCaseData<MockForInherit>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsGood>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsAsterisks>(testCase)],
                [CreateTestCaseData<IMockForInherit>(testCase, typeof(MockForInherit))],
                [CreateTestCaseData<IMockForInherit_Implemented>(testCase, typeof(MockForInherit))],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterface>(testCase, typeof(MockForInherit))],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceWithSpecsExactName>(testCase, typeof(MockForInherit))],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceWithSpecsAsterisks>(testCase, typeof(MockForInherit))],
            });


    private static TestCaseName[] ThrowingNamesDespiteSetupOk =>
    [
        new("", "empty - will check and return bad"),
        new("WRONG NAME", "incorrect name - will check and return bad"),
        new(nameof(IMockForInherit), "incorrect name - will check and return bad")
    ];

    public IEnumerable<object[]> ThrowingTypesAndNamesDespiteSetupOk()
        => ThrowingNamesDespiteSetupOk
            .SelectMany(testCase => new object[][]
            {
                [CreateTestCaseData<MockForInherit>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsGood>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsAsterisks>(testCase)],
                [CreateTestCaseData<IMockForInherit>(testCase)],
                [CreateTestCaseData<IMockForInherit_NotImplemented>(testCase)],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceForInterface>(testCase)],
            });
    
    public IEnumerable<object[]> OkIfDisableNameCheck()
        => new TestCaseName[] { new("*", "Skip type Check") }
            .SelectMany(testCase => new object[][]
            {
                [CreateTestCaseData<MockForInherit>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsGood>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsAsterisks>(testCase)],
                [CreateTestCaseData<IMockForInherit>(testCase)],
                // Skip because not implemented
                //[CreateTestCaseTypeGenAndName<IMockForInherit_NotImplemented>(testCase)],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceForInterface>(testCase)],
                
                
                [CreateTestCaseData<MockForInheritDerivedBasic>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsBad>(testCase)],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceWithSpecsBad>(testCase)],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceWithSpecsAsIMock>(testCase)],
            });


    private static TestCaseName[] ThrowingNamesBecauseSetupWrong =>
    [
        new(null, "null, do check, but fail because underlying names are wrong"),
        ..ThrowingNamesDespiteSetupOk,
    ];

    public IEnumerable<object[]> ThrowingTypesAndNamesBecauseSetupWrong()
        => ThrowingNamesBecauseSetupWrong
            .SelectMany(testCase => new object[][] {
                [CreateTestCaseData<MockForInheritDerivedBasic>(testCase)],
                [CreateTestCaseData<MockForInheritDerivedSpecsBad>(testCase)],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceWithSpecsBad>(testCase)],
                [CreateTestCaseData<IMockForInherit_ReApplyingInterfaceWithSpecsAsIMock>(testCase)],
            });


    private TestCaseTypeAndName CreateTestCaseData<TModel>(TestCaseName testCase, Type? expected = null) where TModel : class, IModelFromEntity
        => new(testCase.Name, () => this.GetModel<TModel>(testCase.Name), expected ?? typeof(TModel), testCase.Notes);
}
