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
            // first ensure that neither parent nor the sub-item are null
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (parent?.RootContentsForEqualityCheck is null) return false;

            // then verify that the underlying entities have the same reference
            return ReferenceEquals(parent.RootContentsForEqualityCheck, dynObj?.RootContentsForEqualityCheck);
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
            if (obj is null) return false;
            if (ReferenceEquals(parent, obj)) return true;
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
            // check most basic case - they are really the same object or both null
            => !(d1 is null) && (ReferenceEquals(d1, d2) || Equals(d1.RootContentsForEqualityCheck, d2?.RootContentsForEqualityCheck));
    }
}
