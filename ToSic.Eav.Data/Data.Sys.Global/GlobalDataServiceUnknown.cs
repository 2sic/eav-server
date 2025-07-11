﻿using System.Collections.Immutable;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Data.Sys.Global;
internal class GlobalDataServiceUnknown(WarnUseOfUnknown<GlobalDataServiceUnknown> _): IGlobalDataService
{
    public IContentType? GetContentType(string name)
        => null;

    public IContentType? GetContentTypeIfAlreadyLoaded(string name)
        => null;

    public IImmutableList<IEntity> ListRequired
        => [];
}
