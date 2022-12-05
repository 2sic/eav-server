using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    public partial class PropertyStack
    {
        public PropReqResult InternalGetPath(PropReqSpecs specs, PropertyLookupPath path)
        {
            var l = specs.LogOrNull.Fn<PropReqResult>(specs.Field);
            path = path.KeepOrNew();
            var fields = specs.Field.Split('.');
            var currentSource = this as IPropertyLookup;
            PropReqResult result = null;
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                result = currentSource.FindPropertyInternal(specs.ForOtherField(field), path);

                // If nothing found, stop here and return
                if (result.Result == null)
                    return l.Return(result.AsFinal(0), $"nothing found on {field}");

                var isLastKey = index == fields.Length - 1;

                if (isLastKey) return l.Return(result, "last hit, found something");

                // If we got a sub-list and still have keys in the path to check, update the source
                if (result.Result is IEnumerable<IPropertyLookup> resultToStartFrom)
                {
                    // todo: unclear what should be done when there is nothing in the list
                    // it should probably start looking in the parent again...?
                    // normally the first hit would do this, but if we don't have a first hit, it's unclear what should happen

                    currentSource = resultToStartFrom.FirstOrDefault();
                    if (currentSource == null)
                        return l.Return(PropReqResult.NullFinal(result.Path), "found EMPTY list of lookups; will stop");
                    continue;
                }

                // If we got any other value, but would still have fields to check, we must stop now
                // and report there was nothing to find
                if (index < fields.Length - 1)
                    return PropReqResult.NullFinal(result.Path);
            }

            return result ?? PropReqResult.NullFinal(null);
        }
        
    }
}
