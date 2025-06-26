using System.Collections;

namespace ToSic.Eav.Data.Sys.Relationships;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRelatedEntitiesValue
{
    IEnumerable Identifiers { get; }
    int Count { get; }
}