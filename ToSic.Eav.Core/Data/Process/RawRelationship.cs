using System.Collections.Generic;

namespace ToSic.Eav.Data.Process
{
    public class RawRelationship /* <TKey> */: IRawRelationship
    {
        public RawRelationship(List</*TKey*/string> keys)
        {
            Keys = keys;
        }

        public RawRelationship( /*TKey*/ string key)
        {
            Keys = new List</*TKey*/string> { key };
        }

        public List</*TKey*/string> Keys { get; }
    }
}
