using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Decorators
{
    internal class MetadataForDecorator: EntityBasedType
    {
        public static string MetadataForDecoratorId = "4c88d78f-5f3e-4b66-95f2-6d63b7858847";
        public static string MetadataForTargetTypeField = "TargetType";
        public static string MetadataForTargetNameField = "TargetName";
        public static string MetadataForDeleteWarningField = "DeleteWarning";

        public MetadataForDecorator(IEntity entity) : base(entity)
        {
        }
    }
}
