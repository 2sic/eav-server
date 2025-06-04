using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.PropertyStack.Sys;

partial class PropertyStackNavigator
{
    private PropReqResult GetResultOfSibling(PropReqSpecs specs, ILog logOrNull, PropertyLookupPath path)
    {
        var l = logOrNull.Fn<PropReqResult>(specs.Dump());
        var earliestNextSibling = StackAddress.Index + 1;
        path = path.Add("StackSibling", earliestNextSibling.ToString(), specs.Field);
        var sibling = StackAddress.Source.GetNextInStack(specs, earliestNextSibling, path);
        if (sibling is not { IsFinal: true })
            return l.Return(PropReqResult.Null(path), $"no useful sibling with '{specs.Field}' found");

        // Log & Return
        l.A($"Another sibling found. Name:{sibling.Name} #{sibling.SourceIndex}. Will try to check it's properties. ");
            
        var stackWrapper = new StackReWrapper(StackAddress.NewWithOtherIndex(sibling.SourceIndex), logOrNull);
        return l.Return(stackWrapper.ReWrapIfPossible(sibling), "ok");
    }
        
}