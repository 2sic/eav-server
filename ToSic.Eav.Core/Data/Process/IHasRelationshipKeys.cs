using System.Collections.Generic;

namespace ToSic.Eav.Data.Process
{
    public interface IHasRelationshipKeys
    {
        // TODO: MAKE ienumerable
        List<object> RelationshipKeys { get; }
    }
}
