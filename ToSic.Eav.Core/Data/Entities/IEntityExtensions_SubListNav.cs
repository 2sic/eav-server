using System;
using System.Linq;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    // ReSharper disable once InconsistentNaming
    public static partial class IEntityExtensions
    {
        /// <summary>
        /// Special case on entity lists v12.03
        /// If nothing was found so far, try to see if we could find a child-entity with a title matching the field
        /// </summary>
        /// <returns></returns>
        public static PropertyRequest TryToNavigateToEntityInList(this IEntity entity, string field, object parentDynEntity, ILog parentLogOrNull)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull("Sxc.SubLst");
            var safeWrap = logOrNull.SafeCall<PropertyRequest>();

            var dynChildField = entity.Type?.DynamicChildrenField;
            if (string.IsNullOrEmpty(dynChildField)) return safeWrap("no dyn-child", null);


            var children = entity.Children(dynChildField);
            if (children == null) return safeWrap("no child", null);
            // if (!(childField is DynamicEntity dynamicChild)) return safeWrap("child not DynamicEntity", null);
            if (children.First().EntityId == 0) return safeWrap("Child is placeholder, no real entries", null);


            try
            {
                var dynEntityWithTitle = children
                    .FirstOrDefault(de => field.Equals(de.GetBestTitle(), StringComparison.InvariantCultureIgnoreCase));

                if (dynEntityWithTitle == null) return safeWrap("no matching child", null);

                // Forward debug state if it's active
                // if(parentLogOrNull != null) dynEntityWithTitle.SetDebug(true);
                //if (parentDynEntity is IPropertyStackLookup parentStack)
                //    dynEntityWithTitle = new EntityWithStackNavigation(dynEntityWithTitle, parentStack, field, 0);

                var result = new PropertyRequest
                {
                    FieldType = DataTypes.Entity,
                    Name = field,
                    Result = dynEntityWithTitle,
                    Source = parentDynEntity,
                    SourceIndex = 0
                };

                return safeWrap("named-entity", result);
            }
            catch
            {
                return safeWrap("error", null);
            }
        }
    }
}
