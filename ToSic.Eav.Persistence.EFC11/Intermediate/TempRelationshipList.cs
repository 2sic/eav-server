using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Intermediate
{
    internal class TempRelationshipList
    {
        public int AttributeId;

        public string StaticName;

        public IEnumerable<int?> Children;
    }
}
