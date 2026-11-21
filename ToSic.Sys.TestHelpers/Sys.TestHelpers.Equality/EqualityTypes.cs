namespace ToSic.Sys.TestHelpers.Equality;


/// <summary>
/// Types of equality comparison.
/// </summary>
/// <remarks>
/// Names are same length on purpose, to make it easier to read the implementations (alignment)
/// </remarks>
public enum EqualityTypes
{
    /// <summary>
    /// XUnit Assert.Equal
    /// </summary>
    AssertEqual,

    /// <summary>
    /// operator ==
    /// </summary>
    /// <remarks>
    /// When testing an NotEqual with the Operator Equal, it will check if it is == is false.
    /// </remarks>
    OperatorEqual,

    /// <summary>
    /// operator !=
    /// </summary>
    /// <remarks>
    /// When testing an Equal with the Operator EqualNegated, it will check if it is != is false.
    /// </remarks>
    OperatorEqualNegated,

    /// <summary>
    /// Object.Equals
    /// </summary>
    ObjectEquals,

    /// <summary>
    /// ReferenceEquals
    /// </summary>
    ReferenceEquals,
}