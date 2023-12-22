using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Decorators;

internal class MetadataExpectedDecorator: ForExpectedBase
{
    // Informs what Metadata is expected / used on a specific item
    public static string ContentTypeNameId = "c490b369-9cd2-4298-af74-19c1e438cdfc";
    public static string ContentTypeName = "MetadataExpectedDecorator";

    public MetadataExpectedDecorator(IEntity entity) : base(entity) { }

    public string Types => Get("MetadataTypes", "");

}