﻿namespace ToSic.Eav.Data.Sys.PropertyStack;

/// <summary>
/// Should be an entity-reader which has a stack of entities it tries to access and prioritize which ones are to be asked first.
/// </summary>
[PrivateApi("internal only - don't publish in docs, can change at any time")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPropertyStackLookup
{
    PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path);

}