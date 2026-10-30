using System.Collections.Immutable;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{

    public IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false) 
        => Deserialize(UnpackEntityAndTestGenericJsonV1(serialized), allowDynamic, skipUnknownType);

    public IEntity DeserializeWithRelsWip(string serialized, int id, bool allowDynamic = false, bool skipUnknownType = false, IEntitiesSource? dynRelationshipsSource = null)
    {
        var jsonEntity = UnpackEntityAndTestGenericJsonV1(serialized) with
        {
            Id = id,
        };
        var entity = Deserialize(jsonEntity, allowDynamic, skipUnknownType, dynRelationshipsSource);
        return entity;
    }



    public IEntity Deserialize(JsonEntity jEnt, bool allowDynamic, bool skipUnknownType, IEntitiesSource? dynRelationshipsSource = default)
    {
        var l = LogDsDetails.Fn<IEntity>($"guid: {jEnt.Guid}; allowDynamic:{allowDynamic} skipUnknown:{skipUnknownType}", timer: true);
        // get type def - use dynamic if dynamic is allowed OR if we'll skip unknown types
        var contentType = JsonDeserializeEntityHelper.GetContentType(jEnt.Type.Id)
                          ?? (allowDynamic || skipUnknownType
                              ? JsonDeserializeEntityHelper.GetTransientContentType(jEnt.Type.Name, jEnt.Type.Id)
                              : throw new FormatException($"type not found for deserialization and dynamic not allowed - cannot continue with {jEnt.Type.Id}"));

        // Metadata Target
        var target = DeserializeEntityTarget(jEnt);

        // Metadata Items - it's important that they are null, if no metadata was specified
        // This is to ensure that without own metadata, it will access the app (if available) to find metadata.
        l.A($"found metadata: {jEnt.Metadata != null}, will deserialize");
        var mdItems = jEnt.Metadata?
            .Select(m => Deserialize(m, allowDynamic, skipUnknownType))
            .ToListOpt();

        // Fix for CS9174: Use a constructible type like ImmutableDictionary instead of IReadOnlyDictionary
        IReadOnlyDictionary<string, IAttribute> attributes = ImmutableDictionary<string, IAttribute>.Empty;

        if (contentType.IsDynamic)
        {
            if (allowDynamic)
                attributes = BuildAttribsOfUnknownContentType(jEnt.Attributes, dynRelationshipsSource);
            else
                l.A("will not resolve attributes because dynamic not allowed, but skip was ok");
        }
        else
        {
            // Note v12.03: even though we're using known types, ATM there are edge cases 
            // with system types where the type is known, but the entities-source is not a full app-state
            // We'll probably correct this some day, but for now we're including the relationshipSource if defined
            attributes = BuildAttribsOfKnownType(jEnt.Attributes, contentType, dynRelationshipsSource);
        }


        l.A("build entity");
        var builder = services.DataAssembler;
        var partsBuilder = builder.EntityConnection.UseOptional(source: DesOptions?.AppReaderOrNull?.AppState, metadata: mdItems);
        var newEntity = services.DataAssembler.Entity.Create(
            appId: DesOptions?.AppId ?? 0,
            guid: jEnt.Guid,
            entityId: jEnt.Id,
            repositoryId: jEnt.Id,
            attributes: attributes,
            metadataFor: target,
            contentType: contentType,
            isPublished: true,
            partsBuilder: partsBuilder,
            created: DateTime.MinValue,
            modified: DateTime.Now,
            owner: jEnt.Owner,
            version: jEnt.Version
        );  


        return l.Return(newEntity, l.Try(() => $"'{newEntity.GetBestTitle()}'", "can't get title"));
    }

}