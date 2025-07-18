﻿using System.Collections.Immutable;
using ToSic.Sys.Data;

namespace ToSic.Eav.Data.Sys.PropertyStack;

/// <summary>
/// This is an entity-reader which has a stack of entities it tries to access and prioritize which ones are to be asked first.
/// </summary>
[PrivateApi("internal only - don't publish in docs, can change at any time")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyStack: IPropertyLookup, IPropertyStackLookup, IHasIdentityNameId
{
    IImmutableList<KeyValuePair<string, IPropertyLookup>> Sources { get; }
        
    IPropertyLookup? GetSource(string name);
        
    IPropertyStack GetStack(params string[] names);
    IPropertyStack GetStack(ILog? log, params string[] names);

    PropReqResult InternalGetPath(PropReqSpecs specs, PropertyLookupPath path);
}