using System.Collections.Generic;

namespace ToSic.Eav.Data.Process
{
    public interface IHasRelationshipKeys
    {
        IEnumerable<object> RelationshipKeys { get; }
    }
}
