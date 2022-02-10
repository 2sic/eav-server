using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Decorators
{
    public class MetadataExpectedDecorator: EntityBasedType
    {

        // Informs what Metadata is expected / used on a specific item
        public static string MetadataExpectedDecoratorId = "c490b369-9cd2-4298-af74-19c1e438cdfc";

        public static string MetadataExpectedTypesField = "MetadataTypes";

        public MetadataExpectedDecorator(IEntity entity) : base(entity)
        {

        }
    }
}
