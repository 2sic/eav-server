using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    // ReSharper disable once InconsistentNaming
    public static partial class IEntityExtensions
    {
        /// <summary>
        /// Special case on entity lists v12.03
        /// Some Content-Types specify a default navigation - this is used extensively in Settings. 
        /// In that case, try to scan the entity for this property and try to find items with the specified title
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
        public static PropertyRequest TryToNavigateToEntityInList(this IEntity entity, string field, object parentDynEntity, ILog parentLogOrNull, PropertyLookupPath path)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull("Sxc.SubLst");
            var safeWrap = logOrNull.SafeCall<PropertyRequest>();

            var dynChildField = entity.Type?.DynamicChildrenField;
            if (string.IsNullOrEmpty(dynChildField)) return safeWrap("no dyn-child", null);


            var children = entity.Children(dynChildField);
            if (children == null) return safeWrap("no child", null);
            if (!children.Any()) return safeWrap("no children", null);
            if (children.First().EntityId == 0) return safeWrap("Child is placeholder, no real entries", null);


            try
            {
                var dynEntityWithTitle = children
                    .FirstOrDefault(de => field.Equals(de.GetBestTitle(), StringComparison.InvariantCultureIgnoreCase));

                if (dynEntityWithTitle == null) return safeWrap("no matching child", null);

                var result = new PropertyRequest
                {
                    FieldType = DataTypes.Entity,
                    Name = field,
                    Result = new List<IEntity> { dynEntityWithTitle },
                    Source = parentDynEntity,
                    SourceIndex = 0,
                    Path = path
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
