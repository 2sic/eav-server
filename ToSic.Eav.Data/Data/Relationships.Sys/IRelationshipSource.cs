//using ToSic.Eav.Apps.State;
using ToSic.Lib.Caching;

namespace ToSic.Eav.Data;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRelationshipSource: ICacheExpiring
{
    /// <summary>
    /// Contains all the relationships of the current app cache.
    /// </summary>
    /*AppRelationshipManager*/ IEnumerable<IEntityRelationship> Relationships { get; }
}