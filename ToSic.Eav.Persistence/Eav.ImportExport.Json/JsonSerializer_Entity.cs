using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Serialization.Sys.Json;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Json;

partial class JsonSerializer
{
    public override string Serialize(IEntity entity) => Serialize(entity, 0);

    public string Serialize(IEntity entity, int metadataDepth)
        => System.Text.Json.JsonSerializer.Serialize(
            new JsonFormat { Entity = ToJson(entity, metadataDepth) },
            JsonOptions.UnsafeJsonWithoutEncodingHtml
        );

    public JsonEntity ToJson(IEntity entity, int metadataDepth = 0)
    {
        var l = Log.Fn<JsonEntity>($"id:{entity?.EntityId}, meta-depth:{metadataDepth}");
        // do a null-check, because sometimes code could ask to serialize not-yet existing entities
        if (entity == null)
            return l.ReturnNull("is null");

        JsonMetadataFor jsonFor = null;
        if (entity.MetadataFor.IsMetadata)
            jsonFor = new()
            {
                // #TargetTypeIdInsteadOfTarget - the Target should become obsolete
                Target = MetadataTargets.GetName(entity.MetadataFor.TargetType),
                TargetType = entity.MetadataFor.TargetType,
                Guid = entity.MetadataFor.KeyGuid,
                Number = entity.MetadataFor.KeyNumber,
                String = entity.MetadataFor.KeyString
            };

        var attributesInUse = entity.Attributes.Values
            .OrderBy(a => a.Name)
            .Where(a => a.Values.Any(v => v.SerializableObject != null))
            .ToList();

        var attribs = new JsonAttributes();
        attributesInUse.GroupBy(a => a.Type, a => a).ToList().ForEach(g =>
        {
            var gList = g.ToList();
            switch (g.Key)
            {
                case ValueTypes.String:
                    attribs.String = ToTypedDictionary<string>(gList);
                    break;
                case ValueTypes.Hyperlink:
                    var links = ToTypedDictionary<string>(gList);
                    attribs.Hyperlink = ValueConvertHyperlinks
                        ? ConvertReferences(links, entity.EntityGuid)
                        : links;
                    break;
                case ValueTypes.Custom:
                    attribs.Custom = ToTypedDictionary<string>(gList);
                    break;
                case ValueTypes.Json:
                    attribs.Json = ToTypedDictionary<string>(gList);
                    break;
                case ValueTypes.Number:
                    attribs.Number = ToTypedDictionary<decimal?>(gList);
                    break;
                case ValueTypes.DateTime:
                    attribs.DateTime = ToTypedDictionary<DateTime?>(gList);
                    break;
                case ValueTypes.Boolean:
                    attribs.Boolean = ToTypedDictionary<bool?>(gList);
                    break;
                case ValueTypes.Entity:
                    attribs.Entity = ToTypedDictionaryEntity(gList);
                    break;
                case ValueTypes.Empty:
                case ValueTypes.Undefined:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        // new: optionally include metadata
        List<JsonEntity> itemMeta = null;
        var metaList = ((entity.Metadata as MetadataOf<Guid>)?.AllWithHidden ?? entity.Metadata as IEnumerable<IEntity>)
            .ToList();
        if (metadataDepth > 0 && metaList.Any())
            itemMeta = metaList.Select(m => ToJson(m, metadataDepth - 1)).ToList();

        var jEnt = new JsonEntity
        {
            Id = entity.EntityId,
            Guid = entity.EntityGuid,
            Version = entity.Version,
            Type = new(entity, withMap: true),
            Attributes = attribs,
            Owner = entity.Owner,
            For = jsonFor,
            Metadata = itemMeta
        };
        return l.ReturnAsOk(jEnt);
    }

    /// <summary>
    /// this is a special helper to create typed entities-dictionaries
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, Dictionary<string, List<Guid?>>> ToTypedDictionaryEntity(List<IAttribute> gList)
    {
        // the following is a bit complex for the following reason
        // 1. either the relationship is guid based, and in that case, 
        //    it's possible that the items don't exist yet (because it's an import or similar)
        // 2. or it's int/id based, in which case the items exist, 
        //    but the relationship manager doesn't have a direct reference to the guid,
        //    but only to the items directly
        // so it tries to get the guids first, and otherwise uses the items
        var entities = ToTypedDictionary<IEnumerable<IEntity>>(gList)
            .ToDictionary(a => a.Key, a => a.Value
                .ToDictionary(b => b.Key, b => ((LazyEntitiesSource)b.Value).ResolveGuids()));
        return entities;
    }

    private static string LanguageKey(IValue v) =>
        string.Join(",", v.Languages
                .OrderBy(l => l.ReadOnly)
                .Select(l => (l.ReadOnly ? ReadOnlyMarker : "") + l.Key)
                .ToArray())
            .UseFallbackIfNoValue(NoLanguage);
}