﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

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
    public class PropertyStackNavigator: IWrapper<IPropertyLookup>, IPropertyStackLookup
    {
        public PropertyStackNavigator(IPropertyLookup child, IPropertyStackLookup parent, string field, int index)
        {
            UnwrappedContents = child;
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


        public IPropertyLookup UnwrappedContents { get; set; }

        public PropertyRequest PropertyInStack(string fieldName, string[] dimensions, int startAtSource, bool treatEmptyAsDefault)
        {
            // Try to find on child
            var childResult = UnwrappedContents.FindPropertyInternal(fieldName, dimensions);
            if (childResult != null)
            {
                // Test if final was already checked, otherwise update it
                if (!childResult.IsFinal) PropertyStack.MarkAsFinalOrNot(childResult, "", 0, treatEmptyAsDefault);
                
                // if it is final, return that
                if (childResult.IsFinal) return childResult;
            }

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
