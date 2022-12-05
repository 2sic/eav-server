using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Data
{
    public partial class PropertyStack
    {
        public PropReqResult InternalGetPath(PropReqSpecs specs, PropertyLookupPath path)
        {
            path = path.KeepOrNew();
            var fields = specs.Field.Split('.');
            var startAt = this as IPropertyLookup;
            PropReqResult result = null;
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                result = startAt.FindPropertyInternal(specs.ForOtherField(field), path);

                // If nothing found, stop here and return
                if (result.Result == null)
                    return result.AsFinal(0);

                // If we got a sub-list and still have keys in the path to check, update the source
                if (result.Result is IEnumerable<IPropertyLookup> resultToStartFrom)
                {
                    startAt = resultToStartFrom.First();
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
