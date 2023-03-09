using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data.Process
{
    public class RawRelationship : IRawRelationship
    {
        public RawRelationship(IEnumerable<object> keys)
        {
            Keys = keys?.ToList() ?? new List<object>();
        }

        public RawRelationship(object key)
        {
            Keys = new List<object> { key };
        }

        public List<object> Keys { get; }
    }
}
