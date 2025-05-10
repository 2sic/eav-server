using System.Collections;

namespace ToSic.Eav.Data;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRelatedEntitiesValue
{
    IList Identifiers { get; }
}