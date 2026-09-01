namespace ToSic.Eav.Models.Inheritances;

/// <summary>
/// Simple test case information.
/// </summary>
/// <param name="Name">Name to apply when trying to get the model</param>
/// <param name="Notes">Additional notes about the test case</param>
public record TestCaseName(
    string? Name,
    string Notes
);

/// <summary>
/// Test case information for a specific type and name, including a generator function and expected type.
/// This is used to ensure that any exceptions thrown happen during the test, not during data-generation.
/// </summary>
/// <param name="Name">Name to apply when trying to get the model</param>
/// <param name="Generator">Function to generate the object for the test case</param>
/// <param name="Notes">Additional notes about the test case</param>
public record TestCaseTypeAndName(
    string? Name,
    Func<object?> Generator,
    Type OriginalType,
    Attribute Attribute,
    string Notes
);

