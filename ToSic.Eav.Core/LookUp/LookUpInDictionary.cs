﻿using System;
using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp;

/// <summary>
/// Look Up values from a .net dictionary. Case-Insensitive. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
/// <param name="name">Name to use</param>
/// <param name="valueList">value list (dictionary) to reference - will convert to invariant; or create blank</param>
[PublicApi]
public class LookUpInDictionary(string name, IDictionary<string, string> valueList = null) : LookUpBase(name)
{
    /// <summary>
    /// List with static properties and Test-Values
    /// </summary>
    public IDictionary<string, string> Properties { get; }
        // either take existing dic or create new, but always make sure it's case-insensitive
        = valueList != null
            ? new(valueList, StringComparer.InvariantCultureIgnoreCase)
            : new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

    /// <inheritdoc/>
    public override string Get(string key, string format)
    {
        // first try a safe check
        if (!Properties.ContainsKey(key)) return string.Empty;

        // then attempt the try/catch way
        try
        {
            return Properties[key] ?? string.Empty;
        }
        catch (KeyNotFoundException)
        {
            return string.Empty;
        }
    }
}