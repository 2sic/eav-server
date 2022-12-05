namespace ToSic.Eav.Data
{
    public partial class PropertyStackNavigator
    {
        //private PropertyRequest GetResultOfSiblingsUsingRecursion(string field, string[] dimensions, int startAtSource, bool treatEmptyAsDefault, ILog logOrNull, PropertyLookupPath path)
        //{
        //    var l = logOrNull.Fn<PropertyRequest>(
        //        $"{nameof(field)}:{field}, {nameof(dimensions)}:{string.Join(",", dimensions)}, {nameof(startAtSource)}:{startAtSource}");

        //    var earliestNextSibling = StackAddress.Index + 1;


        //    path = path.Add("StackSibling", earliestNextSibling.ToString(), StackAddress.Field);
        //    var sibling = StackAddress.Source.GetNextInStack(field, dimensions, earliestNextSibling, true, logOrNull, path);
        //    if (sibling == null || !sibling.IsFinal)
        //        return l.Return(PropertyRequest.Null(path), "no useful sibling found");


        //    // Log to path & log that we will try the next option
        //    path = sibling.Path;    // Keep path as it was generated to find this sibling
        //    l.A($"Another sibling found. Name:{sibling.Name} #{sibling.SourceIndex}. Will try to check it's properties. ");

        //    var stackWrapper = new StackWrapper(StackAddress, logOrNull);

        //    // Check if it's an IEntity or a Wrapper
        //    if (sibling.Result is IEnumerable<IEntity> siblingEntities && siblingEntities.Any())
        //    {
        //        var wrapInner = logOrNull.Fn(null, "It's a list of entities as expected.");
        //        var entityNav = new EntityWithStackNavigation(siblingEntities.First(), StackAddress, NextDepth);
        //        path = path.Add("StackIEntity", field);
        //        var result = entityNav.FindPropertyInternal(field, dimensions, logOrNull, path);
        //        wrapInner.Done();
        //        return l.Return(result);
        //    }

        //    // New: Check if it's a Dictionary
        //    if (sibling.Result is IEnumerable<PropertyLookupDictionary> siblingDics && siblingDics.Any())
        //    {
        //        var wrapInner = logOrNull.Fn(null, "It's a list of entities as expected.");
        //        var entityNav = new PropertyLookupWithStackNavigation(siblingDics.First(), StackAddress, 0);
        //        path = path.Add("StackLookupDictionary", field);
        //        var result = entityNav.FindPropertyInternal(field, dimensions, logOrNull, path);
        //        wrapInner.Done();
        //        return l.Return(result);
        //    }

        //    if (sibling.Result is IEnumerable<IPropertyLookup> siblingStack && siblingStack.Any())
        //    {
        //        l.A("Another sibling found, it's a list of IPropertyLookups.");
        //        var wrapInner = logOrNull.Fn(null, "It's a list of entities as expected.");
        //        var propNav = new PropertyStackNavigator(siblingStack.First(), StackAddress, NextDepth);
        //        path = path.Add("StackIPropertyLookup", field);
        //        var result = propNav.GetNextInStack(field, dimensions, 0, true, logOrNull, path);
        //        wrapInner.Done();
        //        return l.Return(result);
        //    }

        //    // We got here, so we found nothing
        //    // This means result is not final or after checking the parent again, no better source was found
        //    // In this case, return the initial child result, which already said it's not final
        //    return l.Return(null, "no sibling can return data, return empty result");
        //}
    }
}
