using System.Collections;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class PropertyRequestExtensions
{
    /// <summary>
    /// Review the result and mark as final if it is final.
    /// Also optionally log the decision process. 
    /// </summary>
    /// <returns></returns>
    public static PropReqResult MarkAsFinalOrNot(this PropReqResult result, string sourceName, int sourceIndex, ILog logOrNull, bool treatEmptyAsFinal)
    {
        var l = logOrNull.Fn<PropReqResult>();
        // Check nulls and prevent multiple executions
        if (result == null) return l.ReturnNull("null");
        if (result.IsFinal) return l.Return(result, "already final");

        result.Name = sourceName ?? result.Name;


        // if any non-null is ok, use that.
        if (!treatEmptyAsFinal)
            return l.Return(result.AsFinal(sourceIndex), "empty is ok");

        // this may set a null, but may also set an empty string or empty array
        if (result.Result.IsNullOrDefault())
            return l.Return(result, "NullOrDefault - not final");

        if (result.Result is string foundString)
            return string.IsNullOrEmpty(foundString)
                ? l.Return(result, "empty string, not final")
                : l.Return(result.AsFinal(sourceIndex), "string, non-empty - final");

        // Return entity-list if it has elements, otherwise continue searching
        if (result.Result is IEnumerable<IEntity> entityList)
            return !entityList.Any()
                ? l.Return(result, "empty list, not final")
                : l.Return(result.AsFinal(sourceIndex), "list, non empty, final");

        // not sure if this will ever hit
        if (result.Result is ICollection list)
            return list.Count == 0
                ? l.Return(result, "empty collection, not final")
                : l.Return(result.AsFinal(sourceIndex), "list, non-empty, final");

        // All seems ok, special checks passed, return result
        return l.Return(result.AsFinal(sourceIndex), "all ok/final");
    }
}