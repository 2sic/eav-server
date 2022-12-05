using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
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
        public static PropReqResult MarkAsFinalOrNot(this PropReqResult result, string sourceName, int sourceIndex, ILog logOrNull, bool treatEmptyAsDefault)
        {
            var lg = logOrNull.Fn<PropReqResult>();
            // Check nulls and prevent multiple executions
            if (result == null) return lg.ReturnNull("null");
            if (result.IsFinal) return lg.Return(result, "already final");

            result.Name = sourceName ?? result.Name;


            // if any non-null is ok, use that.
            if (!treatEmptyAsDefault)
                return lg.Return(result.AsFinal(sourceIndex), "empty is ok");

            // this may set a null, but may also set an empty string or empty array
            if (result.Result.IsNullOrDefault())
                return lg.Return(result, "NullOrDefault - not final");

            if (result.Result is string foundString)
                return string.IsNullOrEmpty(foundString)
                    ? lg.Return(result, "empty string, not final")
                    : lg.Return(result.AsFinal(sourceIndex), "string, non-empty - final");

            // Return entity-list if it has elements, otherwise continue searching
            if (result.Result is IEnumerable<IEntity> entityList)
                return !entityList.Any()
                    ? lg.Return(result, "empty list, not final")
                    : lg.Return(result.AsFinal(sourceIndex), "list, non empty, final");

            // not sure if this will ever hit
            if (result.Result is ICollection list)
                return list.Count == 0
                    ? lg.Return(result, "empty collection, not final")
                    : lg.Return(result.AsFinal(sourceIndex), "list, non-empty, final");

            // All seems ok, special checks passed, return result
            return lg.Return(result.AsFinal(sourceIndex), "all ok/final");
        }
    }
}
