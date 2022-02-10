using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Decorators
{
    internal class ExpectedDecorator: ForExpectedBase
    {
        // Informs what Metadata is expected / used on a specific item
        public static string TypeGuid = "c490b369-9cd2-4298-af74-19c1e438cdfc";

        public ExpectedDecorator(IEntity entity) : base(entity) { }

        public string Types => Get("MetadataTypes", "");

    }
}
