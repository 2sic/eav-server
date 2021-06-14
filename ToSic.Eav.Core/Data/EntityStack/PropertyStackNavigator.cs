using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public class PropertyStackNavigator: IWrapper<IPropertyLookup>, IPropertyStackLookup
    {
        public PropertyStackNavigator(IPropertyLookup child, IPropertyStackLookup parent, string field, int index)
        {
            UnwrappedContents = child;
            Parent = parent;
            ParentField = field;
            ParentIndex = index;
        }
        public IPropertyStackLookup Parent;
        public string ParentField;
        public int ParentIndex;


        public IPropertyLookup UnwrappedContents { get; set; }

        public PropertyRequest PropertyInStack(string fieldName, string[] dimensions, int startAtSource, bool treatEmptyAsDefault)
        {
            // Try to find on child
            var childResult = UnwrappedContents.FindPropertyInternal(fieldName, dimensions);
            if (childResult.IsFinal) return childResult;

            // If it didn't work, check if parent has another option
            // Not found yet, ask parent if it may have another
            // If the parent has another source, create a new navigator for that and return that result
            // This will in effect have a recursion - if that won't succeed it will ask the parent again.
            var parentResult = Parent.PropertyInStack(ParentField, dimensions, ParentIndex +1, true);
            if (parentResult.IsFinal && parentResult.Result is IEnumerable<IEntity> siblingEntities && siblingEntities.Any())
                return new EntityWithStackNavigation(siblingEntities.First(), Parent, ParentField, parentResult.SourceIndex)
                    .FindPropertyInternal(fieldName, dimensions);
            
            if (parentResult.IsFinal && parentResult.Result is IEnumerable<IPropertyLookup> siblingStack && siblingStack.Any())
                return new PropertyStackNavigator(siblingStack.First(), Parent, ParentField, parentResult.SourceIndex)
                    .PropertyInStack(fieldName, dimensions, 0, true);

            // We got here, so we found nothing
            // This means result is not final or after checking the parent again, no better source was found
            // In this case, return the initial child result, which already said it's not final
            return childResult;
        }
    }
}
