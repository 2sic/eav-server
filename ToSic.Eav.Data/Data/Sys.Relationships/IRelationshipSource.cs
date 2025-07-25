﻿//using ToSic.Eav.Apps.State;

using ToSic.Sys.Caching;

namespace ToSic.Eav.Data.Sys.Relationships;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRelationshipSource: ICacheExpiring
{
    /// <summary>
    /// Contains all the relationships of the current app cache.
    /// </summary>
    /*AppRelationshipManager*/ IEnumerable<IEntityRelationship> Relationships { get; }
}