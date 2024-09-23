using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Data;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IRelationshipSource: ICacheExpiring
{
    /// <summary>
    /// Contains all the relationships of the current app cache.
    /// </summary>
    AppRelationshipManager Relationships { get; }
}