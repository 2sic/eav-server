using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data;

public static class PropertyRequestExtensions
{
    /// <summary>
    /// Review the result and mark as final if it is final.
    /// Also optionally log the decision process. 
    /// </summary>
    /// <returns></returns>
    public static PropReqResult MarkAsFinalOrNot(this PropReqResult result, string sourceName, int sourceIndex,
        ILog logOrNull, bool treatEmptyAsDefault) => logOrNull.Func(l =>
    {
        // Check nulls and prevent multiple executions
        if (result == null) return (null, "null");
        if (result.IsFinal) return (result, "already final");

        result.Name = sourceName ?? result.Name;


        // if any non-null is ok, use that.
        if (!treatEmptyAsDefault)
            return (result.AsFinal(sourceIndex), "empty is ok");

        // this may set a null, but may also set an empty string or empty array
        if (result.Result.IsNullOrDefault())
            return (result, "NullOrDefault - not final");

        if (result.Result is string foundString)
            return string.IsNullOrEmpty(foundString)
                ? (result, "empty string, not final")
                : (result.AsFinal(sourceIndex), "string, non-empty - final");

        // Return entity-list if it has elements, otherwise continue searching
        if (result.Result is IEnumerable<IEntity> entityList)
            return !entityList.Any()
                ? (result, "empty list, not final")
                : (result.AsFinal(sourceIndex), "list, non empty, final");

        // not sure if this will ever hit
        if (result.Result is ICollection list)
            return list.Count == 0
                ? (result, "empty collection, not final")
                : (result.AsFinal(sourceIndex), "list, non-empty, final");

        // All seems ok, special checks passed, return result
        return (result.AsFinal(sourceIndex), "all ok/final");
    });
}