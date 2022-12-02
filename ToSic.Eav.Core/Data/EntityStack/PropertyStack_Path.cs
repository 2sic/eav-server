using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    public partial class PropertyStack
    {
        public PropertyRequest InternalGetPath(string fieldPath, string[] dimensions, ILog parentLogOrNull, PropertyLookupPath path)
        {
            path = path.KeepOrNew();
            var fields = fieldPath.Split('.');
            var startAt = this as IPropertyLookup;
            PropertyRequest result = null;
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                result = startAt.FindPropertyInternal(field, dimensions, parentLogOrNull, path);

                // If nothing found, stop here and return
                if (result.Result == null)
                    return result.AsFinal(0);

                // If we got a sub-list and still have keys in the path to check, update the source
                if (result.Result is IEnumerable<IPropertyLookup> resultToStartFrom)
                    startAt = new PropertyStack().Init(field, resultToStartFrom.ToArray());
                
                // If we got any other value, but would still have fields to check, we must stop now
                // and report there was nothing to find
                else if (index < fields.Length - 1)
                    return NotFound(result.Path);
            }

            return result ?? NotFound(null);
        }

        private PropertyRequest NotFound(PropertyLookupPath path) => new PropertyRequest(null, path)
        {
            FieldType = "NotFound", //wip
        }.AsFinal(0);
    }
}
