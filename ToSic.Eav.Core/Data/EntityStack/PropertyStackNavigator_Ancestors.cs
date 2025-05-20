using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Data;

partial class PropertyStackNavigator
{
    private PropReqResult GetResultOfAncestors(PropReqSpecs specs, ILog logOrNull, PropertyLookupPath path)
    {
        var l = logOrNull.Fn<PropReqResult>(specs.Dump());
        l.A("No sibling result found, will check grand parent");
        var ancestor = StackAddress.Ancestor;
        l.A($"Found Grandparent: {ancestor != null}");
        if (ancestor == null)
            return l.Return(PropReqResult.NullFinal(path), "no ancestor to check");

        path = path.Add("↩️");

        // Try each ancestor to find matching ancestor with matching child
        for (var ancIndex = 0; ancIndex < MaxAncestorSiblings; ancIndex++)  // avoid infinite loops
        {
            (ancestor, var reqResult) = GetAncestorSibling(ancestor, specs, path);

            // If no matching ancestor found, return empty
            if (!reqResult.IsFinal)
                return l.Return(PropReqResult.NullFinal(reqResult.Path), "no more relevant ancestors found");

            // Now try ancestor child
            var innerLookup = reqResult.Result as IPropertyLookup
                              ?? (reqResult.Result as IEnumerable<IPropertyLookup>)?.First();
            if (innerLookup != null)
            {
                // If we have a stack, try that, so we don't go deep into recursions
                var ancestorChild = innerLookup is IPropertyStackLookup stackLookup
                    ? stackLookup.GetNextInStack(specs, 0, path)
                    : innerLookup.FindPropertyInternal(specs, reqResult.Path);

                if (ancestorChild.IsFinal)
                    return l.Return(ancestorChild, "found child of ancestor sibling");
            }
            else
                l.A($"Found ancestor with '{ancestor.Field}' but not a {nameof(IPropertyLookup)} so we can't use; skip");
        }

        return l.Return(PropReqResult.NullFinal(path), "tried max ancestor siblings, nothing found");
    }

    private (StackAddress currentAddress, PropReqResult Request) GetAncestorSibling(
        StackAddress ancestorAddress, PropReqSpecs specs, PropertyLookupPath path
    )
    {
        var ancestorSpecs = specs.ForOtherField(ancestorAddress.Field);
        var ancestorSibling = ancestorAddress.Source.GetNextInStack(ancestorSpecs,
            ancestorAddress.Index + 1, path);
        return (
            ancestorSibling.IsFinal
                ? ancestorAddress.NewWithOtherIndex(ancestorSibling.SourceIndex)
                : ancestorAddress,
            ancestorSibling
        );
    }
}