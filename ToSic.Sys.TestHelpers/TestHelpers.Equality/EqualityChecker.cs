using Xunit.DependencyInjection;
using static Xunit.Assert;

namespace ToSic.Sys.TestHelpers.Equality;

/// <summary>
/// Various objects within the system have equality overloads.
/// We must be able to test equality on all combinations, and this should help.
/// </summary>
/// <remarks>
/// It also offers a feature to disable certain tests.
/// This could be configured in the test constructor.
/// As of 2026-08 where this was introduced, it's not in use, but it was verified.
/// </remarks>
/// <typeparam name="T"></typeparam>
/// <param name="output"></param>
public class EqualityChecker<T>(ITestOutputHelperAccessor output) where T: class
{
    #region Configure Skipping certain tests

    private readonly HashSet<EqualityTypes> _disabledChecks = [];
    
    public EqualityChecker<T> Disable(EqualityTypes equalityType)
    {
        _disabledChecks.Add(equalityType);
        return this;
    }

    private bool ShouldSkip(EqualityTypes equalityType)
    {
        var skip = _disabledChecks.Contains(equalityType);
        if (skip)
            output.Output?.WriteLine($"⏸️ Skipping {equalityType}");
        return skip;
    }

    #endregion


    public void Equal(T md, T md2, EqualityTypes equalityType)
    {
        // Make sure we can verify in the output, that the correct test is performed
        // just to avoid accidental wrong test results, if the test is changed in the future.
        output.Output?.WriteLine($"Qual {equalityType}");
        output.Output?.WriteLine($"First: {md}");
        output.Output?.WriteLine($"Second: {md2}");
        if (ShouldSkip(equalityType))
            return;
        switch (equalityType)
        {
            case EqualityTypes.AssertEqual:
                Assert.Equal(md, md2);
                break;
            case EqualityTypes.OperatorEqual:
                True((dynamic)md == (dynamic)md2);
                break;     // Cast to dynamic, so that the runtime will find the operators even if the type is generic
            case EqualityTypes.OperatorEqualNegated:
                False((dynamic)md != (dynamic)md2);
                break;
            case EqualityTypes.ObjectEquals:
                True((dynamic)md.Equals((dynamic)md2));
                break;
            case EqualityTypes.ReferenceEquals:
                True(ReferenceEquals(md, md2));
                break;
            default: throw new ArgumentOutOfRangeException(nameof(equalityType), equalityType, null);
        }
    }
    
    public void NotEqual(T md, T md2, EqualityTypes equalityType)
    {
        // Make sure we can verify in the output, that the correct test is performed
        // just to avoid accidental wrong test results, if the test is changed in the future.
        output.Output?.WriteLine($"Checking {equalityType}");
        output.Output?.WriteLine($"First: {md}");
        output.Output?.WriteLine($"Second: {md2}");
        if (ShouldSkip(equalityType))
            return;
        switch (equalityType)
        {
            case EqualityTypes.AssertEqual:
                Assert.NotEqual(md, md2);
                break;
            case EqualityTypes.OperatorEqual:
                False((dynamic)md == (dynamic)md2);
                break;
            case EqualityTypes.OperatorEqualNegated:
                True((dynamic)md != (dynamic)md2);
                break;
            case EqualityTypes.ObjectEquals:
                False(md.Equals(md2));
                break;
            case EqualityTypes.ReferenceEquals:
                False(ReferenceEquals(md, md2));
                break;
            default: throw new ArgumentOutOfRangeException(nameof(equalityType), equalityType, null);
        }
    }
}