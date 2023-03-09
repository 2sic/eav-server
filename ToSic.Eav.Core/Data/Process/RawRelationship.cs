using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data.Process
{
    public class RawRelationship : IRawRelationship
    {
        public RawRelationship(
            string noParamOrder = Parameters.Protector,
            object key = default,
            IEnumerable<object> keys = default)
        {
            Parameters.Protect(noParamOrder);
            Keys = keys?.ToList()
                   ?? (key == null ? null : new List<object> { key })
                   ?? new List<object>();
        }

        public List<object> Keys { get; }
    }
}
