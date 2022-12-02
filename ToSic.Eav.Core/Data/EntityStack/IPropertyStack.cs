﻿using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is an entity-reader which has a stack of entities it tries to access and prioritize which ones are to be asked first.
    /// </summary>
    [PrivateApi("internal only - don't publish in docs, can change at any time")]
    public interface IPropertyStack: IPropertyLookup, IPropertyStackLookup
    {
        IImmutableList<KeyValuePair<string, IPropertyLookup>> Sources { get; }
        
        IPropertyLookup GetSource(string name);
        
        IPropertyStack GetStack(params string[] names);
        IPropertyStack GetStack(ILog log, params string[] names);

        PropertyRequest InternalGetPath(string fieldPath, string[] dimensions, ILog parentLogOrNull, PropertyLookupPath path);
    }
}
