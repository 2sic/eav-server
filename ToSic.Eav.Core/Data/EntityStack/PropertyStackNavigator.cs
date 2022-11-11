using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;

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

        // Error constants
        private const int MaxLookupCycles = 1000; // This is the max depth for looking - it must be fairly high, because some lookup increase the depth quickly

        private static string MaxLookupError = $"Error finding value in Stack - too many cycles used (> {MaxLookupCycles}. This is probably a bug in EAV property stack";
        public PropertyStackNavigator(IPropertyLookup child, IPropertyStackLookup parent, string field, int index, int depth): base(child)
        {
            Parent = parent;
            ParentField = field;
            OwnIndexInParent = index;
            Depth = depth;
        }
        
        /// <summary>
        /// The parent LookupStack which would get another source in case this doesn't find something
        /// </summary>
        public readonly IPropertyStackLookup Parent;
        
        /// <summary>
        /// The name in the parent, which resulted in this object being created. 
        /// </summary>
        public readonly string ParentField;
        
        /// <summary>
        /// The index of sources on the parent - because if this source doesn't return something, it must continue on that.
        /// </summary>
        public readonly int OwnIndexInParent;

        public readonly int Depth;


        public PropertyRequest PropertyInStack(string field, string[] dimensions, int startAtSource, bool treatEmptyAsDefault, ILog parentLogOrNull, PropertyLookupPath path)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".PSNav");
            var safeWrap = logOrNull.Fn<PropertyRequest>(
                $"{nameof(field)}:{field}, {nameof(dimensions)}:{string.Join(",", dimensions)}, {nameof(startAtSource)}:{startAtSource}");

            // Catch errors with infinite recursions
            // This shouldn't happen, but if it ever does we don't want the server to run into buffer overflows
            if (path.Parts.Count > 1000)
            {
                safeWrap.A("Maximum lookup depth achieved");
                var err = new PropertyRequest { Result = MaxLookupError, Name = "error", Path = path, Source = "error", SourceIndex = OwnIndexInParent };
                return safeWrap.Return(err, "error");
            }

            PropertyRequest resultOfOwn = null;
            // Try to find on child - but only if startAt == 0
            // If it's > 0, we're coming back from an inner-property not found, and then we should skip this
            if (startAtSource == IndexOfOwnItem) // IndexOfOwnItem = 0
            {
                path = path.Add("StackOwnItem", field);
                resultOfOwn = GetResultOfOwnItem(field, dimensions, logOrNull, treatEmptyAsDefault, path);
                if (resultOfOwn != null && resultOfOwn.IsFinal) return safeWrap.Return(resultOfOwn, "final");
            }

            path = path.Add("↩️");
            safeWrap.A("Couldn't find a result  yet, will retry the parent");

            // If it didn't work, check if parent has another option
            // Not found yet, ask parent if it may have another
            // If the parent has another source, create a new navigator for that and return that result
            // This will in effect have a recursion - if that won't succeed it will ask the parent again.
            var nextIndexOnParent = OwnIndexInParent + 1;
            path = path.Add("StackSibling", nextIndexOnParent.ToString(), ParentField);
            var sibling = Parent.PropertyInStack(ParentField, dimensions, nextIndexOnParent, true, logOrNull, path);
            
            if (sibling == null || !sibling.IsFinal) return safeWrap.Return(new PropertyRequest(), "no useful sibling found");
            
            path = sibling.Path;    // Keep path as it was generated to find this sibling

            safeWrap.A($"Another sibling found. Name:{sibling.Name} #{sibling.SourceIndex}. Will try to check it's properties. ");
            if (sibling.Result is IEnumerable<IEntity> siblingEntities && siblingEntities.Any())
            {
                var wrapInner = logOrNull.Fn(null, "It's a list of entities as expected.");
                var entityNav = new EntityWithStackNavigation(siblingEntities.First(), Parent, ParentField, sibling.SourceIndex, Depth + 1);
                path = path.Add("StackIEntity", field);
                var result = entityNav.FindPropertyInternal(field, dimensions, logOrNull, path);
                wrapInner.Done();
                return safeWrap.Return(result);
            }

            if (sibling.Result is IEnumerable<IPropertyLookup> siblingStack && siblingStack.Any())
            {
                safeWrap.A("Another sibling found, it's a list of IPropertyLookups.");
                var wrapInner = logOrNull.Fn(null, "It's a list of entities as expected.");
                var propNav = new PropertyStackNavigator(siblingStack.First(), Parent, ParentField, sibling.SourceIndex, Depth + 1);
                path = path.Add("StackIPropertyLookup", field);
                var result = propNav.PropertyInStack(field, dimensions, 0, true, logOrNull, path);
                wrapInner.Done();
                return safeWrap.Return(result);
            }
            
            // We got here, so we found nothing
            // This means result is not final or after checking the parent again, no better source was found
            // In this case, return the initial child result, which already said it's not final
            return safeWrap.Return(new PropertyRequest(), "no sibling can return data, return empty result");
        }

        
        /// <summary>
        /// Just get the result of the child which we're wrapping. 
        /// </summary>
        /// <returns></returns>
        private PropertyRequest GetResultOfOwnItem(string field, string[] dimensions, ILog logOrNull, bool treatEmptyAsDefault, PropertyLookupPath path)
        {
            var safeWrap = logOrNull.Fn<PropertyRequest>();

            // 2022-05-02 2dm - there seem to be cases where this wrapper is created without an own entity.
            // Not yet sure why, but in this case we must be sure to not return something.
            if (_contents == null) return safeWrap.ReturnNull("no entity");

            path = path.Add("OwnItem", field);
            var childResult = _contents.FindPropertyInternal(field, dimensions, logOrNull, path);
            if (childResult == null) return safeWrap.ReturnNull("null");
            
            // Test if final was already checked, otherwise update it
            if (!childResult.IsFinal)
                childResult.MarkAsFinalOrNot(null, 0, logOrNull, treatEmptyAsDefault);

            // if it is final, return that
            if (!childResult.IsFinal) return safeWrap.Return(childResult, "not final");
            
            // test if the returned stuff is one or more entities, in which case they should implement stack-fallback
            if (!(childResult.Result is IEnumerable<IEntity> entList)) return safeWrap.Return(childResult, "not null/entities, final");

            safeWrap.A("Result is IEnumerable<IEntity> - will wrap in navigation");
            var entArray = entList.ToArray();
            if (entArray.Any() && !(entArray.First() is EntityWithStackNavigation))
                childResult.Result =
                    entArray.Select(e => new EntityWithStackNavigation(e, this, field, IndexOfNextItem, Depth + 1));

            return safeWrap.Return(childResult, "entities, final");
        }
    }
    
    
}
