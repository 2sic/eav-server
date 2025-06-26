﻿
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Sys.PropertyDump;

public class EntityDump : IPropertyDumper
{
    public int IsCompatible(object target) =>
        target switch
        {
            Entity => 100,
            EntityWrapper => 100,
            IEntity => 50,
            _ => 0
        };

    public List<PropertyDumpItem> Dump(object target, PropReqSpecs specs, string path, IPropertyDumpService dumpService)
        => DumpTyped(target as IEntity ?? throw new ArgumentException("should be an IEntity", nameof(target)), specs, path, dumpService);

    private List<PropertyDumpItem> DumpTyped(IEntity entity, PropReqSpecs specs, string path, IPropertyDumpService dumpService)
    {
        entity = entity is EntityWrapper ew ? ew.Entity : entity;

        if (!entity.Attributes.Any())
            return [];
        if (PropertyDumpItem.ShouldStop(path))
            return [PropertyDumpItem.DummyErrorShouldStop(path)];

        var pathRoot = string.IsNullOrEmpty(path)
            ? ""
            : path + PropertyDumpItem.Separator;

        // Check if we have dynamic children
        IEnumerable<PropertyDumpItem>? resultDynChildren = null;
        var dynChildField = entity.Type?.DynamicChildrenField;
        if (dynChildField != null)
            resultDynChildren = entity.Children(dynChildField)
                .Where(child => child != null)
                .SelectMany(inner => dumpService.Dump(inner!, specs, pathRoot + inner!.GetBestTitle(specs.Dimensions))
                // #DropUseOfDumpProperties
                // ?? inner._DumpNameWipDroppingMostCases(specs, pathRoot + inner.GetBestTitle(specs.Dimensions))
                );

        // Get all properties which are not dynamic children
        var childAttributes = entity.Attributes
            .Where(att =>
                att.Value.Type == ValueTypes.Entity
                && (dynChildField == null || !att.Key.Equals(dynChildField, StringComparison.InvariantCultureIgnoreCase))
            );

        var resultProperties = childAttributes
            .SelectMany(att => entity
                .Children(att.Key)
                .Where(child => child != null) // apparently sometimes the entities inside seem to be non-existent on Resources
                .SelectMany(inner => dumpService.Dump(inner!, specs, pathRoot + att.Key)
                // #DropUseOfDumpProperties
                // ?? inner._DumpNameWipDroppingMostCases(specs, pathRoot + att.Key)
                )
            )
            .ToList();

        // Get all normal properties
        var resultValues = entity.Attributes
                .Where(att => att.Value.Type != ValueTypes.Entity && att.Value.Type != ValueTypes.Empty)
                .Select(att =>
                {
                    var property = entity.FindPropertyInternal(specs.ForOtherField(att.Key), new PropertyLookupPath().Add("EntityDump"));
                    var item = new PropertyDumpItem
                    {
                        Path = pathRoot + att.Key,
                        Property = property
                    };
                    return item;
                })
                .ToList();

        var finalResult = resultProperties.Concat(resultValues);
        if (resultDynChildren != null)
            finalResult = finalResult.Concat(resultDynChildren);

        return finalResult.OrderBy(f => f.Path).ToList();

    }

}