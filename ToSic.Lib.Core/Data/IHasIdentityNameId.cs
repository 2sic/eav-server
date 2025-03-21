﻿using ToSic.Lib.Documentation;

namespace ToSic.Lib.Data;

/// <summary>
/// Special interface to ensure consistency across the code base.
/// It's meant to ensure that any data which has an identity can also provide a string-based ID from that (could be a Guid.ToString()) or a real unique name.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IHasIdentityNameId
{
    /// <summary>
    /// Primary identifier of an object which has this property.
    /// It will be unique and used as an ID where needed.
    /// </summary>
    public string NameId { get; }
}