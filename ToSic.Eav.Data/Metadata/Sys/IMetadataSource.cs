﻿using ToSic.Eav.Apps;
using ToSic.Sys.Caching;

namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// For querying metadata from the data source.
/// Mainly used in the Store, Cache-Systems and Apps.
/// </summary>
[PrivateApi("Till v18 Was InternalApi_DoNotUse_MayChangeWithoutNotice, but the public one is now IMetadataGet")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IMetadataSource: 
    ICacheExpiring, 
    IAppIdentity,    // this is used for creating additional metadata on this source
    Metadata.IMetadataSource,
    IMetadataOfSource; // new v18, moved to the primary MetadataSource for now