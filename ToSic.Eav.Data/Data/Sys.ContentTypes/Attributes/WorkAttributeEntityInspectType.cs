namespace ToSic.Eav.Data.Sys.ContentTypes;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkAttributeEntityInspectType(): ServiceBase("Eav.AtInTy")
{
    public string PrimaryTypeName(IContentTypeAttribute definition, bool modeCreate, bool tryOtherModes = false)
    {
        var l = Log.Fn<string>($"attribute: {definition.Name}; {nameof(modeCreate)}: {modeCreate}; {nameof(tryOtherModes)}: {tryOtherModes}");
        var name = PrimaryTypeNames(definition, modeCreate, tryOtherModes).FirstOrDefault() ?? "";
        return l.Return(name, $"first on {nameof(modeCreate)} {modeCreate}: '{name}'");
    }

    public IList<string> PrimaryTypeNames(IContentTypeAttribute definition, bool modeCreate, bool tryOtherModes = false)
    {
        var l = Log.Fn<IList<string>>($"attribute: {definition.Name}; {nameof(modeCreate)}: {modeCreate}; {nameof(tryOtherModes)}: {tryOtherModes}");
        // Make sure it's the right initial type
        if (!definition.IsEntity())
            return l.Return([], "empty");
        
        // First check if it's a picker
        // In this case, the values are stored differently for create vs. data
        if (definition.IsPicker())
        {
            l.A("is picker");
            var sources = definition
                .GetPickerDataSources()
                .ToList();
            var list = modeCreate
                ? sources.GetPickerCreateTypeNames()
                : sources.GetPickerDataTypeNames();

            if (list.Any())
                return RetAndLog(list, $"first on {nameof(modeCreate)} {modeCreate}");

            if (tryOtherModes)
            {
                l.A("Found nothing, try other mode.");
                list = !modeCreate
                    ? sources.GetPickerCreateTypeNames()
                    : sources.GetPickerDataTypeNames();

                if (list.Any())
                    return RetAndLog(list, $"first on {nameof(modeCreate)} {!modeCreate}");
            }
        }
        
        // Do basic check for non-pickers
        var itemTypeName = definition.Metadata.Get<string>(AttributeNames.EntityFieldType) ?? "";
        var typeNames = itemTypeName.CsvToArrayWithoutEmpty();
        return RetAndLog(typeNames, $"{typeNames}");

        IList<string> RetAndLog(IList<string> list, string message)
            => l.Return(list, message + ": " + string.Join(",", list));
    }
}