﻿using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.Data.PropertyStack.Sys;

namespace ToSic.Eav.LookUp.Sources.Sys;

/// <summary>
/// TODO
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class LookUpInStack: LookUpIn<IPropertyStack>
{
    private readonly string?[] _dimensions;

    /// <summary>
    /// Constructs a new Entity LookUp
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dimensions">the languages / dimensions to use</param>
    public LookUpInStack(IPropertyStack source, string?[]? dimensions): base(source, source.NameId?.ToLowerInvariant() ?? "stack-unknown", "LookUp in stack")
    {
        _dimensions = dimensions ?? IZoneCultureResolverExtensions.SafeLanguagePriorityCodes(null);
    }

    /// <summary>
    /// Constructs a new Entity LookUp
    /// </summary>
    /// <param name="name">Name of the LookUp, e.g. Settings</param>
    /// <param name="source"></param>
    /// <param name="dimensions">the languages / dimensions to use</param>
    public LookUpInStack(string name, IPropertyStack source, string?[]? dimensions): base(source, name)
    {
        _dimensions = dimensions ?? IZoneCultureResolverExtensions.SafeLanguagePriorityCodes(null);
    }

    // todo: might need to clarify what language/culture the key is taken from in an entity
    /// <summary>
    /// Special lookup command with format-provider.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public override string Get(string key, string format)
    {
        // Return empty string if Entity is null
        if (Data == null!)
            return string.Empty;

        if (!key.HasValue())
            return string.Empty;

        // Make sure that in case the key uses ":" separators, we neutralize back to standard "."
        key = key.Replace(":", ".");

        // Try to just find, and format the result if all is ok
        var propReqResult = Data.InternalGetPath(new(key, _dimensions, true), new());
        if (propReqResult.Result != null)
            return LookUpHelpers.FormatValue(propReqResult.Result, format, _dimensions);

        return string.Empty;

        //// Not found yet, so check for Navigation-Property (e.g. Manager:Name)
        //var subTokens = CheckAndGetSubToken(key);
        //if (!subTokens.HasSubtoken)
        //    return string.Empty;
        //valueObject = Data.GetBestValue(subTokens.Source, _dimensions);
        //if (valueObject == null)
        //    return string.Empty;

        //// Finally: Handle child-Entity-Field (sorted list of related entities)
        //if (!(valueObject is IEnumerable<IEntity> relationshipList))
        //    return string.Empty;
        //var first = relationshipList.FirstOrDefault();
        //return first == null
        //    ? string.Empty
        //    : new LookUpInEntity("no-name", first, _dimensions).Get(subTokens.Rest, format);
    }
}