
namespace ToSic.Eav.Data;

/// <summary>
/// Contains comparison methods for all Entity Wrappers
/// This is important, because otherwise == or GroupBy see the various wrappers as different objects
/// </summary>
public static class MultiWrapperEquality
{
    /// <summary>
    /// This is used for a fast-compare if objects might be the same.
    /// It's usually just an initial check, followed by Equals checks
    /// </summary>
    public static int GetWrappedHashCode<T>(IMultiWrapper<T> parent) => parent.RootContentsForEqualityCheck?.GetHashCode() ?? 0;

    /// <summary>
    /// Check if two wrappers are holding the same entity
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool EqualsWrapper<T>(IMultiWrapper<T> a, IMultiWrapper<T> b) where T : class
    {
        // Check the root objects first and independently
        // Since if either is null and the other is not, then they can't be equal
        // 2023-08-08 previously this was different, but IMHO wrong
        if (a is null) return b is null;
        if (b is null) return false;
        // first ensure that neither parent nor the sub-item are null
        // ReSharper disable once ConvertIfStatementToReturnStatement
        var aContents = a.RootContentsForEqualityCheck;
        var bContents = b.RootContentsForEqualityCheck;
        if (aContents is null) return bContents is null;

        // then verify that the underlying entities have the same reference
        return ReferenceEquals(aContents, bContents);
    }

    /// <summary>
    /// Check if two objects are the same - assuming they are wrappers
    /// and that in that case the underlying _EntityForEqualityCheck will be verified
    /// Used for Equals calls on child objects
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool EqualsObj<T>(IMultiWrapper<T> a, object b) where T : class
    {
        if (b is null) return a is null; // 2023-08-08 previously returned false every time, which caused problems on null-checks;
        if (ReferenceEquals(a, b)) return true;
        // This is a check if it's exactly that type, but it could be an inherited type, so false must fall to the next check
        if (b is T && a.RootContentsForEqualityCheck.Equals(b)) return true;
        return b is IMultiWrapper<T> wrapper && EqualsWrapper(a, wrapper);
    }

    /// <summary>
    /// Check if they are equal, based on the underlying entity. 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <remarks>
    /// It's important to do null-checks first, because if anything in here is null, it will otherwise throw an error. 
    /// But we can't use != null, because that would call the != operator and be recursive.
    /// </remarks>
    /// <returns></returns>
    public static bool IsEqual<T>(IMultiWrapper<T> a, IMultiWrapper<T> b) where T : class
    {
        // 2023-08-08 previously returned false if d1 was null (without comparing d2)
        if (a is null) return b is null;
        if (b is null) return false;

        // check most basic case - they are really the same object or both null - or the same applies to their inner contents
        return ReferenceEquals(a, b) || Equals(a.RootContentsForEqualityCheck, b?.RootContentsForEqualityCheck);
    }
}