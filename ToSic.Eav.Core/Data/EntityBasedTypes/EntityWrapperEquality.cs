namespace ToSic.Eav.Data
{
    /// <summary>
    /// Contains comparison methods for all Entity Wrappers
    /// This is important, because otherwise == or GroupBy see the various wrappers as different objects
    /// </summary>
    public static class EntityWrapperEquality
    {
        /// <summary>
        /// This is used for a fast-compare if objects might be the same.
        /// It's usually just an initial check, followed by Equals checks
        /// </summary>
        public static int GetHashCode(IEntityWrapper parent) => parent.EntityForEqualityCheck?.GetHashCode() ?? 0;

        /// <summary>
        /// Check if two wrappers are holding the same entity
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="dynObj"></param>
        /// <returns></returns>
        public static bool EqualsWrapper(IEntityWrapper parent, IEntityWrapper dynObj)
        {
            // first ensure that neither parent nor the sub-item are null
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (parent?.EntityForEqualityCheck is null) return false;

            // then verify that the underlying entities have the same reference
            return ReferenceEquals(parent.EntityForEqualityCheck, dynObj?.EntityForEqualityCheck);
        }

        /// <summary>
        /// Check if two objects are the same - assuming they are wrappers
        /// and that in that case the underlying _EntityForEqualityCheck will be verified
        /// Used for Equals calls on child objects
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool EqualsObj(IEntityWrapper parent, object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(parent, obj)) return true;
            return obj is IEntityWrapper wrapper && EqualsWrapper(parent, wrapper);
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
        public static bool IsEqual(IEntityWrapper d1, IEntityWrapper d2)
            // check most basic case - they are really the same object or both null
            => !(d1 is null) && (ReferenceEquals(d1, d2) || Equals(d1?.EntityForEqualityCheck, d2?.EntityForEqualityCheck));
    }
}
