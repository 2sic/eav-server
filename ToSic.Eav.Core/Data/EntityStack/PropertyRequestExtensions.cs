using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data
{
    public static class PropertyRequestExtensions
    {
        /// <summary>
        /// Review the result and mark as final if it is final.
        /// Also optionally log the decision process. 
        /// </summary>
        /// <returns></returns>
        public static PropertyRequest MarkAsFinalOrNot(this PropertyRequest result, string sourceName, int sourceIndex, ILog logOrNull, bool treatEmptyAsDefault)
        {
            var safeWrap = logOrNull.SafeCall<PropertyRequest>();
            // Check nulls and prevent multiple executions
            if (result == null) return safeWrap("null", null);
            if (result.IsFinal) return safeWrap("already final", result);

            result.Name = sourceName ?? result.Name;


            // if any non-null is ok, use that.
            if (!treatEmptyAsDefault)
                return safeWrap("empty is ok", result.AsFinal(sourceIndex));

            // this may set a null, but may also set an empty string or empty array
            if (result.Result.IsNullOrDefault(treatFalseAsDefault: false))
                return safeWrap("NullOrDefault - not final", result);

            if (result.Result is string foundString)
                return string.IsNullOrEmpty(foundString)
                    ? safeWrap("empty string, not final", result)
                    : safeWrap("string, non-empty - final", result.AsFinal(sourceIndex));

            // Return entity-list if it has elements, otherwise continue searching
            if (result.Result is IEnumerable<IEntity> entityList)
                return !entityList.Any()
                    ? safeWrap("empty list, not final", result)
                    : safeWrap("list, non empty, final", result.AsFinal(sourceIndex));

            // not sure if this will ever hit
            if (result.Result is ICollection list)
                return list.Count == 0
                    ? safeWrap("empty collection, not final", result)
                    : safeWrap("list, non-empty, final", result.AsFinal(sourceIndex));

            // All seems ok, special checks passed, return result
            return safeWrap("all ok/final", result.AsFinal(sourceIndex));
        }
    }
}
