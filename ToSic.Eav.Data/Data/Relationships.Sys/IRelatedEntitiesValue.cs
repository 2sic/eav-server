using System.Collections;

namespace ToSic.Eav.Data.Relationships.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRelatedEntitiesValue
{
    IList Identifiers { get; }
}