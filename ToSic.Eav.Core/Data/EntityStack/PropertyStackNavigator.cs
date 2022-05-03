using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
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
    public class PropertyStackNavigator: Wrapper<IPropertyLookup>, IPropertyStackLookup
    {
        private const int IndexOfOwnItem = 0;
        private const int IndexOfNextItem = 1;
        public PropertyStackNavigator(IPropertyLookup child, IPropertyStackLookup parent, string field, int index): base(child)
        {
            Parent = parent;
            ParentField = field;
            ParentIndex = index;
        }
        
        /// <summary>
        /// The parent LookupStack which would get another source in case this doesn't find something
        /// </summary>
        public IPropertyStackLookup Parent;
        
        /// <summary>
        /// The name in the parent, which resulted in this object being created. 
        /// </summary>
        public string ParentField;
        
        /// <summary>
        /// The index of sources on the parent - because if this source doesn't return something, it must continue on that.
        /// </summary>
        public int ParentIndex;


        public PropertyRequest PropertyInStack(string field, string[] dimensions, int startAtSource, bool treatEmptyAsDefault, ILog parentLogOrNull, PropertyLookupPath path)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".PSNav");
            var safeWrap = logOrNull.SafeCall<PropertyRequest>(
                $"{nameof(field)}:{field}, {nameof(dimensions)}:{string.Join(",", dimensions)}, {nameof(startAtSource)}:{startAtSource}");

            PropertyRequest childResult = null;
            // Try to find on child - but only if startAt == 0
            // If it's > 0, we're coming back from an inner-property not found, and then we should skip this
            if (startAtSource == IndexOfOwnItem)
            {
                path = path.Add("StackChild", field);
                childResult = GetResultOfChild(field, dimensions, logOrNull, treatEmptyAsDefault, path);
                if (childResult != null && childResult.IsFinal) return safeWrap("final", childResult);
            }

            path = path.Add("↩️");
            logOrNull.SafeAdd("Couldn't find a result  yet, will retry the parent");

            // If it didn't work, check if parent has another option
            // Not found yet, ask parent if it may have another
            // If the parent has another source, create a new navigator for that and return that result
            // This will in effect have a recursion - if that won't succeed it will ask the parent again.
            path = path.Add("StackSibling", (ParentIndex + 1).ToString(), ParentField);
            var sibling = Parent.PropertyInStack(ParentField, dimensions, ParentIndex + 1, true, logOrNull, path);
            
            if (sibling == null || !sibling.IsFinal) return safeWrap("no useful sibling found", childResult);
            
            logOrNull.SafeAdd($"Another sibling found. Name:{sibling.Name} #{sibling.SourceIndex}. Will try to check it's properties. ");
            if (sibling.Result is IEnumerable<IEntity> siblingEntities && siblingEntities.Any())
            {
                var wrapInner = logOrNull.SafeCall(null, "It's a list of entities as expected.");
                var entityNav = new EntityWithStackNavigation(siblingEntities.First(), Parent, ParentField, sibling.SourceIndex);
                path = path.Add("StackIEntity", field);
                var result = entityNav.FindPropertyInternal(field, dimensions, logOrNull, path);
                wrapInner(null);
                return safeWrap(null, result);
            }

            if (sibling.Result is IEnumerable<IPropertyLookup> siblingStack && siblingStack.Any())
            {
                logOrNull.SafeAdd("Another sibling found, it's a list of IPropertyLookups.");
                var wrapInner = logOrNull.SafeCall(null, "It's a list of entities as expected.");
                var propNav = new PropertyStackNavigator(siblingStack.First(), Parent, ParentField, sibling.SourceIndex);
                path = path.Add("StackIPropertyLookup", field);
                var result = propNav.PropertyInStack(field, dimensions, 0, true, logOrNull, path);
                wrapInner(null);
                return safeWrap(null, result);
            }
            
            // We got here, so we found nothing
            // This means result is not final or after checking the parent again, no better source was found
            // In this case, return the initial child result, which already said it's not final
            return safeWrap("no sibling can return data, return empty result", childResult);
        }

        
        /// <summary>
        /// Just get the result of the child which we're wrapping. 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="dimensions"></param>
        /// <param name="logOrNull"></param>
        /// <param name="treatEmptyAsDefault"></param>
        /// <returns></returns>
        private PropertyRequest GetResultOfChild(string field, string[] dimensions, ILog logOrNull, bool treatEmptyAsDefault, PropertyLookupPath path)
        {
            var safeWrap = logOrNull.SafeCall<PropertyRequest>();

            // 2022-05-02 2dm - there seem to be cases where this wrapper is created without an own entity.
            // Not yet sure why, but in this case we must be sure to not return something.
            if (_contents == null) return safeWrap("no entity", null);

            path = path.Add("Child", field);
            var childResult = _contents.FindPropertyInternal(field, dimensions, logOrNull, path);
            if (childResult == null) return safeWrap("null", null);
            
            // Test if final was already checked, otherwise update it
            if (!childResult.IsFinal)
                childResult.MarkAsFinalOrNot(null, 0, logOrNull, treatEmptyAsDefault);

            // if it is final, return that
            if (!childResult.IsFinal) return safeWrap("not final", childResult);
            
            // test if the returned stuff is one or more entities, in which case they should implement stack-fallback
            if (!(childResult.Result is IEnumerable<IEntity> entList)) return safeWrap("not null/entities, final", childResult);

            logOrNull?.SafeAdd("Result is IEnumerable<IEntity> - will wrap in navigation");
            var entArray = entList.ToArray();
            if (entArray.Any() && !(entArray.First() is EntityWithStackNavigation))
                childResult.Result =
                    entArray.Select(e => new EntityWithStackNavigation(e, this, field, IndexOfNextItem));

            return safeWrap("entities, final", childResult);
        }
    }
    
    
}
