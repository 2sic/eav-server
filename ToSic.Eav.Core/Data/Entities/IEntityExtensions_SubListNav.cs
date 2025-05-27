using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Plumbing;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Data;

// ReSharper disable once InconsistentNaming
static partial class IEntityExtensions
{
    /// <summary>
    /// Special case on entity lists v12.03
    /// Some Content-Types specify a default navigation - this is used extensively in Settings. 
    /// In that case, try to scan the entity for this property and try to find items with the specified title
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static PropReqResult TryToNavigateToEntityInList(this IEntity entity, PropReqSpecs specs,
        object parentDynEntity, PropertyLookupPath path) 
    {
        var l = specs.LogOrNull.Fn<PropReqResult>(specs.Field);
        var field = specs.Field;
        // Check if we have a configuration for dynamic children
        var dynChildField = entity.Type?.DynamicChildrenField;
        if (string.IsNullOrEmpty(dynChildField))
            return l.ReturnNull("no dyn-child");

        // Check if the children are in any way relevant
        var children = entity.Children(dynChildField);
        if (children == null) return l.ReturnNull("no child");
        if (!children.Any()) return l.ReturnNull("no children");
        if (children.First() == null) return l.ReturnNull("child is null");
        if (children.First().EntityId == 0) return l.ReturnNull("Child is placeholder, no real entries");


        try
        {
            // Find possible child with the correct title
            var dynEntityWithTitle = children.FirstOrDefault(e => field.EqualsInsensitive(e.GetBestTitle()));

            if (dynEntityWithTitle == null) return l.ReturnNull("no matching child");

            var result = new PropReqResult(result: new List<IEntity> { dynEntityWithTitle }, valueType: ValueTypesWithState.Entity, path: path)
            {
                Name = field,
                Source = parentDynEntity,
                SourceIndex = 0,
            };

            return l.Return(result, "named-entity");
        }
        catch
        {
            return l.ReturnNull("error");
        }
    }
}