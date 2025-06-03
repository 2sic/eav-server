using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Data;

namespace ToSic.Eav.Data;

/// <summary>
/// This is an internal object and it does quite some complex stuff, so we must document it.
/// Image if Razor requests the following setting:
/// * Settings.GoogleMaps.ApiKey
///
/// Now imagine if the AppSettings has a `GoogleMaps` object which only overrides the zoom, but not the Key. In that case this _would_ happen
/// 1. Razor would successfully get Settings.GoogleMaps
/// 1. It would then ask for the ApiKey and turn up empty
///
/// In reality, this should happen:
/// 1. If the ApiKey is empty, it should _backtrack_ back to the `Settings` object and find out if there is another GoogleMaps settings provider
/// 1. Then ask for that for an ApiKey
/// 1. Repeat till found
///
/// This is what this object does:
/// * It wraps the first returned `GoogleMaps` object and will check if it has the desired `ApiKey` property
/// * If not found, it won't report that back, but ask the Parent for another GoogleMaps object...
/// * And let that take over - it may in turn repeat the process again
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class PropertyStackNavigator(IPropertyLookup child, StackAddress stackAddress)
    : Wrapper<IPropertyLookup>(child), IPropertyStackLookup
{
    private const int MaxAncestorSiblings = 10;

    // Error constants
    private const int MaxLookupCycles = 1000; // This is the max depth for looking - it must be fairly high, because some lookup increase the depth quickly

    private static string MaxLookupError = $"Error finding value in Stack - too many cycles used (> {MaxLookupCycles}. This is probably a bug in EAV property stack";

    public StackAddress StackAddress { get; } = stackAddress;


    public PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path)
    {
        var logOrNull = specs.LogOrNull.SubLogOrNull(EavLogs.Eav + ".PSNav");
        var l = logOrNull.Fn<PropReqResult>($"{specs.Dump()}, {nameof(startAtSource)}:{startAtSource}");

        // Catch errors with infinite recursions
        // This shouldn't happen, but if it ever does we don't want the server to run into buffer overflows
        if (path.Parts.Count > 1000)
            return l.Return(
                new(result: MaxLookupError, valueType: ValueTypesWithState.Error, path: path)
                    { Name = "error", Source = "error", SourceIndex = StackAddress.Index },
                "Error: Maximum lookup depth achieved");

        // Try to find on child - but only if startAt == 0
        // If it's > 0, we're coming back from an inner-property not found, and then we should skip this
        if (startAtSource == 0)
        {
            path = path.Add("StackOwnItem", specs.Field);
            var resultOfOwn = GetResultOfOwnItem(specs, logOrNull, path);
            if (resultOfOwn != null && resultOfOwn.IsFinal) 
                return l.Return(resultOfOwn, "final");
        }

        // Log to path & log that we will try the next option
        path = path.Add("↩️");
        l.A("Couldn't find a result  yet, will retry the siblings parent");

        // If it didn't work, check if parent has another option
        // Not found yet, ask parent if it may have another
        // If the parent has another source, create a new navigator for that and return that result
        // This will in effect have a recursion - if that won't succeed it will ask the parent again.

        var siblingResult = GetResultOfSibling(specs, logOrNull, path);

        if (siblingResult.IsFinal) return l.Return(siblingResult, "sibling result");

        l.A("No sibling result found, will check grand parent");
        var ancestorResult = GetResultOfAncestors(specs, logOrNull, path);
                 
        return l.Return(ancestorResult, ancestorResult.IsFinal
            ? "no sibling or ancestor can return data, return empty result"
            : "ancestor sibling result");
    }



    /// <summary>
    /// Just get the result of the child which we're wrapping. 
    /// </summary>
    /// <returns></returns>
    private PropReqResult GetResultOfOwnItem(PropReqSpecs specs, ILog logOrNull, PropertyLookupPath path)
    {
        var l = logOrNull.Fn<PropReqResult>();
        // 2022-05-02 2dm - there seem to be cases where this wrapper is created without an own entity.
        // Not yet sure why, but in this case we must be sure to not return something.
        if (GetContents() == null)
            return l.ReturnNull("no entity/contents");

        path = path.Add("OwnItem", specs.Field);
        var childResult = GetContents().FindPropertyInternal(specs, path);
        if (childResult == null)
            return l.ReturnNull("null");
            
        // Test if final was already checked, otherwise update it
        if (!childResult.IsFinal)
            childResult.MarkAsFinalOrNot(null, 0, logOrNull, specs.TreatEmptyAsDefault);

        // if it is final, return that
        if (!childResult.IsFinal)
            return l.Return(childResult, "not final");

        // TODO: @2dm - this doesn't look right yet, why are we making "this" the new parent?
        var maybeAdjustedParent = StackAddress.Child(this, specs.Field, childResult.SourceIndex);
        var reWrapper = new StackReWrapper(maybeAdjustedParent, logOrNull);

        var final = reWrapper.ReWrapIfPossible(childResult);
        return l.Return(final, final.ResultOriginal != null ? "re-wrapped" : "not null/entities, final");
    }
}