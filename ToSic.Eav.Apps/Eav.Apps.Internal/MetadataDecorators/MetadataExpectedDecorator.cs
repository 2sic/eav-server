using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

internal class MetadataExpectedDecorator(IEntity entity) : ForExpectedBase(entity)
{
    // Informs what Metadata is expected / used on a specific item
    public static string ContentTypeNameId = "c490b369-9cd2-4298-af74-19c1e438cdfc";
    public static string ContentTypeName = "MetadataExpectedDecorator";

    public string Types => Get("MetadataTypes", "");

}