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
            var safeWrap = logOrNull.Call2<PropertyRequest>();

            var dynChildField = entity.Type?.DynamicChildrenField;
            if (string.IsNullOrEmpty(dynChildField)) return safeWrap.ReturnNull("no dyn-child");


            var children = entity.Children(dynChildField);
            if (children == null) return safeWrap.ReturnNull("no child");
            if (!children.Any()) return safeWrap.ReturnNull("no children");
            if (children.First().EntityId == 0) return safeWrap.ReturnNull("Child is placeholder, no real entries");


            try
            {
                var dynEntityWithTitle = children
                    .FirstOrDefault(de => field.Equals(de.GetBestTitle(), StringComparison.InvariantCultureIgnoreCase));

                if (dynEntityWithTitle == null) return safeWrap.ReturnNull("no matching child");

                var result = new PropertyRequest
                {
                    FieldType = DataTypes.Entity,
                    Name = field,
                    Result = new List<IEntity> { dynEntityWithTitle },
                    Source = parentDynEntity,
                    SourceIndex = 0,
                    Path = path
                };

                return safeWrap.Return(result, "named-entity");
            }
            catch
            {
                return safeWrap.ReturnNull("error");
            }
        }
    }
}
