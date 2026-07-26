using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.ContentTypes.Fields.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
// ReSharper disable once InconsistentNaming
public static class IContentTypeFieldPickerExtensions
{
    /// <param name="definition"></param>
    extension(IContentTypeField definition)
    {
        /// <summary>
        /// Determine if it's a picker entity-type.
        /// </summary>
        /// <returns></returns>
        public bool IsPicker() =>
            AttributeNames.PickerNames.Contains(definition.InputType);

        /// <summary>
        /// Find all entities which define a data-source
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEntity> GetPickerDataSources() =>
            definition.Metadata
                .SelectMany(e => e.Children(AttributeNames.PickerFieldDataSources))
                .Where(ds => ds != null)
                .Cast<IEntity>();
        
    }

    extension(IEnumerable<IEntity> dataSources)
    {
        public IList<string> GetPickerCreateTypeNames() =>
            GetPickerTypeNames(dataSources, AttributeNames.PickerDataSourceCreateTypes);

        public IList<string> GetPickerDataTypeNames() =>
            GetPickerTypeNames(dataSources, AttributeNames.PickerDataSourceDataTypes);
    }

    private static IList<string> GetPickerTypeNames(IEnumerable<IEntity> dataSources, string field) =>
        dataSources
            .Select(ds => ds.Get<string>(field, languages: []))
            .Where(s => s.HasValue())
            // TODO: INFO @SDV - he probably has comma separated values
            .SelectMany(s => s!
                // TODO!!! NEW-LINES seem to be saved wrong!
                .Replace("\\n", "\n")
                .LinesToArrayWithoutEmpty())
            .ToListOpt();
}