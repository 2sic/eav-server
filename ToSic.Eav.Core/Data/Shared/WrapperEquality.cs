using ToSic.Lib.Data;

namespace ToSic.Eav.Data.Shared
{
    public static class WrapperEquality
    {
        /// <summary>
        /// This is used for a fast-compare if objects might be the same.
        /// It's usually just an initial check, followed by Equals checks
        /// </summary>
        public static int GetHashCode<T>(IWrapper<T> parent) => parent.GetContents()?.GetHashCode() ?? 0;

        /// <summary>
        /// Check if two wrappers are holding the same entity
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool EqualsWrapper<T>(IWrapper<T> a, IWrapper<T> b) where T : class
        {
            // Check the root objects first and independently
            // Since if either is null and the other is not, then they can't be equal
            // 2023-08-08 previously this was different, but IMHO wrong
            if (a is null) return b is null;
            if (b is null) return false;
            // first ensure that neither parent nor the sub-item are null
            // ReSharper disable once ConvertIfStatementToReturnStatement
            var aContents = a.GetContents();
            var bContents = b.GetContents();
            if (aContents is null) return bContents is null;

            // then verify that the underlying entities have the same reference
            return ReferenceEquals(aContents, bContents);
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
        public static bool IsEqual<T>(IWrapper<T> a, IWrapper<T> b) where T : class
        {
            if (a is null) return b is null;
            if (b is null) return false;

            // check most basic case - they are really the same object or both null - or the same applies to their inner contents
            return ReferenceEquals(a, b) || ReferenceEquals(a.GetContents(), b.GetContents());
        }
    }
}
