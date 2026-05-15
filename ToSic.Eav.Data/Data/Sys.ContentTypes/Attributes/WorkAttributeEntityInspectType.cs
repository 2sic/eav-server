namespace ToSic.Eav.Data.Sys.ContentTypes;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkAttributeEntityInspectType(): ServiceBase("Eav.AtInTy")
{
    public string PrimaryTypeName(IContentTypeAttribute definition, bool create = true)
    {
        // Make sure it's the right initial type
        if (!definition.IsEntity())
            return "";
        
        // First check if it's a picker
        // In this case, the values are stored differently for create vs. data
        if (definition.IsPicker())
        {
            var sources = definition
                .GetPickerDataSources();
            var list = create
                ? sources.GetPickerCreateTypeNames()
                : sources.GetPickerDataTypeNames();

            if (list.Any())
                return list.First();
        }
        
        // Do basic check for non-pickers
        var itemTypeName = definition.Metadata.Get<string>(AttributeNames.EntityFieldType) ?? "";
        var typeName = itemTypeName.CsvToArrayWithoutEmpty().FirstOrDefault() ?? "";
        return typeName;
    }

}
