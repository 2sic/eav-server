using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data;

public partial class PropertyStackNavigator
{
    private PropReqResult GetResultOfSibling(PropReqSpecs specs, ILog logOrNull, PropertyLookupPath path) => logOrNull.Func(specs.Dump(), l =>
    {
        var earliestNextSibling = StackAddress.Index + 1;
        path = path.Add("StackSibling", earliestNextSibling.ToString(), specs.Field);
        var sibling = StackAddress.Source.GetNextInStack(specs, earliestNextSibling, path);
        if (sibling == null || !sibling.IsFinal)
            return (PropReqResult.Null(path), $"no useful sibling with '{specs.Field}' found");

        // Log & Return
        l.A($"Another sibling found. Name:{sibling.Name} #{sibling.SourceIndex}. Will try to check it's properties. ");
            
        var stackWrapper = new StackReWrapper(StackAddress.NewWithOtherIndex(sibling.SourceIndex), logOrNull);
        return (stackWrapper.ReWrapIfPossible(sibling), "ok");
    });
        
}