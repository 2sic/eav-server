using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.Serialization;
using ToSic.Eav.Serialization.Sys.Options;

namespace ToSic.Eav.DataSources;

internal class SerializationSourceToDecorator
{
    internal static EntitySerializationDecorator Create(Serialization ds)
    {
        var id = TryParseIncludeRule(ds.IncludeId);
        var title = TryParseIncludeRule(ds.IncludeTitle);
        var guid = TryParseIncludeRule(ds.IncludeGuid);
        var created = TryParseIncludeRule(ds.IncludeCreated);
        var modified = TryParseIncludeRule(ds.IncludeModified);
        var appId = TryParseIncludeRule(ds.IncludeAppId);
        var zoneId = TryParseIncludeRule(ds.IncludeZoneId);

        var dropNullValues = TryParseIncludeRule(ds.RemoveNullValues) ?? false;
        var dropZeroValues = TryParseIncludeRule(ds.RemoveZeroValues) ?? false;
        var dropEmptyStringValues = TryParseIncludeRule(ds.RemoveEmptyStrings) ?? false;
        var dropFalseValues = TryParseIncludeRule(ds.DropFalseValues) ?? false;

        var mdForSer = new MetadataForSerialization
        {
            Serialize = TryParseIncludeRule(ds.IncludeMetadataFor),
            SerializeKey = TryParseIncludeRule(ds.IncludeMetadataForId),
            SerializeType = TryParseIncludeRule(ds.IncludeMetadataForType),
        };

        var mdSer = new SubEntitySerialization
        {
            Serialize = TryParseIncludeRule(ds.IncludeMetadata),
            SerializeId = TryParseIncludeRule(ds.IncludeMetadataId),
            SerializeGuid = TryParseIncludeRule(ds.IncludeMetadataGuid),
            SerializeTitle = TryParseIncludeRule(ds.IncludeMetadataTitle)
        };


        var relSer = new SubEntitySerialization
        {
            Serialize = TryParseIncludeRule(ds.IncludeRelationships),
            // Serialize as CSV can be null, false, true, "array"
            SerializeFormat = GetOutputFormat(ds.IncludeRelationshipsAsCsv),

            SerializeId = TryParseIncludeRule(ds.IncludeRelationshipId),
            SerializeGuid = TryParseIncludeRule(ds.IncludeRelationshipGuid),
            SerializeTitle = TryParseIncludeRule(ds.IncludeRelationshipTitle)
        };

        var typeSer = new TypeSerializationOptions
        {
            SerializeAs = ds.IncludeTypeAs,
            SerializeId = TryParseIncludeRule(ds.IncludeTypeId),
            SerializeName = TryParseIncludeRule(ds.IncludeTypeName),
            SerializeDescription = null,
            PropertyNames = ds.TypePropertyNames
        };

        var decorator = new EntitySerializationDecorator
        {
            RemoveNullValues = dropNullValues,
            RemoveZeroValues = dropZeroValues,
            RemoveEmptyStringValues = dropEmptyStringValues,
            RemoveBoolFalseValues = dropFalseValues,

            // Metadata & Relationships
            SerializeMetadataFor = mdForSer,
            SerializeMetadata = mdSer,
            SerializeRelationships = relSer,
            SerializeType = typeSer,

            // id, title, guid
            SerializeId = id,
            SerializeTitle = title,
            SerializeGuid = guid,
            SerializeAppId = appId,
            SerializeZoneId = zoneId,

            // dates
            SerializeCreated = created,
            SerializeModified = modified
        };
        return decorator;
    }

    private static string GetOutputFormat(string includeRelationshipsAsCsv)
    {
        if (includeRelationshipsAsCsv.IsEmptyOrWs())
            return null;

        var csvAsBool = TryParseIncludeRule(includeRelationshipsAsCsv);
        return csvAsBool switch
        {
            true => "csv",
            false => null,
            _ => includeRelationshipsAsCsv, // could be "array"
        };
    }

    private static bool? TryParseIncludeRule(string original)
        => bool.TryParse(original, out var include) ? include : null;
}