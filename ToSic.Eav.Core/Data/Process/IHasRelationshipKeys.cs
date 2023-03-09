using System.Collections.Generic;

namespace ToSic.Eav.Data.Process
{
    public interface IHasRelationshipKeys
    {
        List<object> RelationshipKeys { get; }
    }
}
