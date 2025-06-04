using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data;

partial class PropertyStack
{
    public PropReqResult InternalGetPath(string path, ILog logOrNull = null)
        => InternalGetPath(new(path, PropReqSpecs.EmptyDimensions, true, logOrNull), new PropertyLookupPath());

    public PropReqResult InternalGetPath(PropReqSpecs specs, PropertyLookupPath path)
        => TraversePath(specs, path.KeepOrNew(), this, NameId);

    public const char PathSeparator = '.';

    public static string[] SplitPathIntoParts(string path, string prefixToIgnore = default)
    {
        if (path == null) return [];
        var parts = path.Split(PathSeparator);
        return prefixToIgnore != default && parts.Any() && prefixToIgnore.EqualsInsensitive(parts.First())
            ? parts.Skip(1).ToArray()
            : parts;
    }

    [PrivateApi]
    public static PropReqResult TraversePath(PropReqSpecs specs, PropertyLookupPath path,
        IPropertyLookup initialSource, string prefixToIgnore = null)
    {
        var l = specs.LogOrNull.Fn<PropReqResult>(specs.Field);
        var fields = SplitPathIntoParts(specs.Field, prefixToIgnore);
        PropReqResult result = null;
        var currentSource = initialSource;

        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            result = currentSource.FindPropertyInternal(specs.ForOtherField(field), path);

            // If nothing found, stop here and return
            if (result.Result == null)
                return l.Return(result.AsFinal(0), $"nothing found on {field}");

            var isLastKey = i == fields.Length - 1;

            if (isLastKey)
                return l.Return(result, "last hit, found something");

            // If we got a sub-list and still have keys in the path to check, update the source
            if (result.Result is IEnumerable<IPropertyLookup> resultToStartFrom)
            {
                // todo: unclear what should be done when there is nothing in the list
                // it should probably start looking in the parent again...?
                // normally the first hit would do this, but if we don't have a first hit, it's unclear what should happen

                currentSource = resultToStartFrom.FirstOrDefault();
                if (currentSource == null)
                    return l.Return(PropReqResult.NullFinal(result.Path), "found EMPTY list of lookups; will stop");
                continue;
            }

            // 2023-09-04 new case with json/anonymous objects: Result is a object with HasPropertyLookup
            if (result.Result is IHasPropLookup hasPropLookup)
            {
                currentSource = hasPropLookup.PropertyLookup;
                if (currentSource == null)
                    return l.Return(PropReqResult.NullFinal(result.Path), "found sub-object without more lookups; will stop");
                continue;
            }

            // If we got any other value, but would still have fields to check, we must stop now
            // and report there was nothing to find
            if (i < fields.Length - 1)
                return l.Return(PropReqResult.NullFinal(result.Path), "stop, nothing to find");
        }

        return l.Return(result ?? PropReqResult.NullFinal(null), "stop");
    }
}