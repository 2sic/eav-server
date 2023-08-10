
namespace ToSic.Eav.Data
{
    /// <summary>
    /// Contains comparison methods for all Entity Wrappers
    /// This is important, because otherwise == or GroupBy see the various wrappers as different objects
    /// </summary>
    public static class WrapperEquality
    {
        /// <summary>
        /// This is used for a fast-compare if objects might be the same.
        /// It's usually just an initial check, followed by Equals checks
        /// </summary>
        public static int GetHashCode<T>(IMultiWrapper<T> parent) => parent.RootContentsForEqualityCheck?.GetHashCode() ?? 0;

        /// <summary>
        /// Check if two wrappers are holding the same entity
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="dynObj"></param>
        /// <returns></returns>
        public static bool EqualsWrapper<T>(IMultiWrapper<T> parent, IMultiWrapper<T> dynObj) where T : class
        {
            // Check the root objects first and independently
            // Since if either is null and the other is not, then they can't be equal
            // 2023-08-08 previously this was different, but IMHO wrong
            if (parent is null) return dynObj is null;
            if (dynObj is null) return false;
            // first ensure that neither parent nor the sub-item are null
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (parent.RootContentsForEqualityCheck is null) return dynObj.RootContentsForEqualityCheck is null;

            // then verify that the underlying entities have the same reference
            return ReferenceEquals(parent.RootContentsForEqualityCheck, dynObj.RootContentsForEqualityCheck);
        }

        /// <summary>
        /// Check if two objects are the same - assuming they are wrappers
        /// and that in that case the underlying _EntityForEqualityCheck will be verified
        /// Used for Equals calls on child objects
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool EqualsObj<T>(IMultiWrapper<T> parent, object obj) where T : class
        {
            if (obj is null) return parent is null; // 2023-08-08 previously returned false every time, which caused problems on null-checks;
            if (ReferenceEquals(parent, obj)) return true;
            // This is a check if it's exactly that type, but it could be an inherited type, so false must fall to the next check
            if (obj is T && parent.RootContentsForEqualityCheck.Equals(obj)) return true;
            return obj is IMultiWrapper<T> wrapper && EqualsWrapper(parent, wrapper);
        }

        /// <summary>
        /// Check if they are equal, based on the underlying entity. 
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <remarks>
        /// It's important to do null-checks first, because if anything in here is null, it will otherwise throw an error. 
        /// But we can't use != null, because that would call the != operator and be recursive.
        /// </remarks>
        /// <returns></returns>
        public static bool IsEqual<T>(IMultiWrapper<T> d1, IMultiWrapper<T> d2) where T : class
            => d1 is null
                // 2023-08-08 previously returned false if d1 was null (without comparing d2)
                ? d2 is null
                // check most basic case - they are really the same object or both null - or the same applies to their inner contents
                : !(d2 is null) && (ReferenceEquals(d1, d2) || Equals(d1.RootContentsForEqualityCheck, d2?.RootContentsForEqualityCheck));
    }
}
