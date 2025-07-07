using System.Collections.Immutable;

using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Metadata.Targets;
using ToSic.Eav.Serialization.Sys.Json;
using static System.StringComparer;


namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{

    public IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false) 
        => Deserialize(UnpackEntityAndTestGenericJsonV1(serialized), allowDynamic, skipUnknownType);

    internal IEntity DeserializeWithRelsWip(string serialized, int id, bool allowDynamic = false, bool skipUnknownType = false, IEntitiesSource? dynRelationshipsSource = null)
    {
        var jsonEntity = UnpackEntityAndTestGenericJsonV1(serialized) with
        {
            Id = id,
        };
        var entity = Deserialize(jsonEntity, allowDynamic, skipUnknownType, dynRelationshipsSource);
        return entity;
    }

    public JsonEntity UnpackEntityAndTestGenericJsonV1(string? serialized)
    {
        var format = UnpackAndTestGenericJsonV1(serialized);
        return format.Entity ?? throw new("No entity found in the package.");
    }

    public JsonFormat UnpackAndTestGenericJsonV1(string? serialized)
    {
        var l = LogDsDetails.Fn<JsonFormat>();

        if (string.IsNullOrWhiteSpace(serialized))
            throw LogException(new ArgumentNullException(nameof(serialized), @"cannot deserialize json - empty or null string"));

        JsonFormat jsonObj;
        try
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            jsonObj = System.Text.Json.JsonSerializer.Deserialize<JsonFormat>(serialized!, JsonOptions.UnsafeJsonWithoutEncodingHtml)!;
        }
        catch (Exception ex)
        {
            throw LogException(new FormatException("cannot deserialize json - bad format", ex));
        }

        if (jsonObj._.V != 1)
            throw LogException(new ArgumentOutOfRangeException(nameof(serialized), $@"unexpected format version: '{jsonObj._.V}'"));

        return l.Return(jsonObj);

        Exception LogException(Exception ex)
        {
            // In case of an error, do make sure that we do actually log it - even if the log-details was null
            var errLogger = l ?? Log.Fn<JsonFormat>();
            return errLogger.Done(ex);
        }
    }

    public IEntity Deserialize(JsonEntity jEnt, bool allowDynamic, bool skipUnknownType, IEntitiesSource? dynRelationshipsSource = default)
    {
        var l = LogDsDetails.Fn<IEntity>($"guid: {jEnt.Guid}; allowDynamic:{allowDynamic} skipUnknown:{skipUnknownType}", timer: true);
        // get type def - use dynamic if dynamic is allowed OR if we'll skip unknown types
        var contentType = GetContentType(jEnt.Type.Id)
                          ?? (allowDynamic || skipUnknownType
                              ? GetTransientContentType(jEnt.Type.Name, jEnt.Type.Id)
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
        var partsBuilder = EntityPartsLazy.ForAppAndOptionalMetadata(source: AppReaderOrNull?.AppState, metadata: mdItems);
        var newEntity = Services.DataBuilder.Entity.Create(
            appId: AppId,
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

    private Target DeserializeEntityTarget(JsonEntity jEnt)
    {
        var l = LogDsDetails.Fn<Target>(timer: true);
        if (jEnt.For == null)
            return l.Return(new(), "no for found");

        var mdFor = jEnt.For;

        var targetType = mdFor.TargetType != 0
            ? mdFor.TargetType
            : MetadataTargets.GetId(mdFor.Target ?? ""); // #TargetTypeIdInsteadOfTarget, should be deprecated soon

        var target = new Target(
            targetType: targetType,
            title: null,
            keyString: mdFor.String,
            keyNumber: mdFor.Number,
            keyGuid: mdFor.Guid
        );

        return l.Return(target, $"this is metadata; will construct 'For' object. Type: {mdFor.Target} ({mdFor.TargetType})");
    }

    private IReadOnlyDictionary<string, IAttribute> BuildAttribsOfUnknownContentType(JsonAttributes jAttributes, IEntitiesSource? relationshipsSource = null)
    {
        var l = LogDsDetails.Fn<IReadOnlyDictionary<string, IAttribute>>(timer: true);
        var valBuilder = Services.DataBuilder.Value;
        var attribs = new[]
        {
            BuildAttrib(jAttributes.DateTime, ValueTypes.DateTime, valBuilder.DateTime),
            BuildAttrib(jAttributes.Boolean, ValueTypes.Boolean, valBuilder.Bool),
            BuildAttrib(jAttributes.Custom, ValueTypes.Custom, valBuilder.String),
            BuildAttrib(jAttributes.Json, ValueTypes.Json, valBuilder.String),
            BuildAttrib(jAttributes.Entity, ValueTypes.Entity, (v, _) => valBuilder.Relationship(v, relationshipsSource)),
            BuildAttrib(jAttributes.Hyperlink, ValueTypes.Hyperlink, valBuilder.String),
            BuildAttrib(jAttributes.Number, ValueTypes.Number, valBuilder.Number),
            BuildAttrib(jAttributes.String, ValueTypes.String, valBuilder.String)
        };
        var final = attribs
            .Where(dic => dic != null)
            .SelectMany(dic => dic!)
            .ToImmutableDicSafe(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);

        return l.ReturnAsOk(final);
    }

    private Dictionary<string, IAttribute>? BuildAttrib<T>(
        Dictionary<string, Dictionary<string, T>>? list,
        ValueTypes type,
        Func<T, IImmutableList<ILanguage>, IValue> valueBuilder)
    {
        if (list == null)
            return null;

        var builder = Services.DataBuilder;
        var newAttributes = list.ToDictionary(
            a => a.Key,
            attrib => builder.Attribute.Create(
                attrib.Key,
                type,
                attrib.Value
                    .Select(v => valueBuilder(v.Value, RecreateLanguageList(v.Key)))
                    .ToListOpt()
            ),
            InvariantCultureIgnoreCase);


        return newAttributes;
    }

    private IReadOnlyDictionary<string, IAttribute> BuildAttribsOfKnownType(JsonAttributes jAttributes, IContentType contentType, IEntitiesSource? relationshipsSource = null)
    {
        var l = LogDsDetails.Fn<IReadOnlyDictionary<string, IAttribute>>();
        var result = contentType.Attributes
            .ToImmutableDicSafe(
                a => a.Name,
                a =>
                {
                    var values = GetValues(a, jAttributes, relationshipsSource);
                    return Services.DataBuilder.Attribute.Create(a.Name, a.Type, values);
                },
                InvariantCultureIgnoreCase
            );
        return l.ReturnAsOk(result);
    }

    private IList<IValue> GetValues(IContentTypeAttribute a, JsonAttributes jAttribs, IEntitiesSource? relationshipsSource)
        => a.Type switch
        {
            ValueTypes.Boolean => BuildValues(jAttribs.Boolean, a),
            ValueTypes.DateTime => BuildValues(jAttribs.DateTime, a),
            ValueTypes.Entity => jAttribs.Entity == null || !jAttribs.Entity.ContainsKey(a.Name)
                ? new List<IValue>() // just keep the empty definition, as that's fine
                : jAttribs.Entity[a.Name]
                    .Select(v => Services.DataBuilder.Value.Relationship(
                        v.Value, relationshipsSource ?? LazyRelationshipLookupList))
                    .ToListOpt(),
            ValueTypes.Hyperlink => BuildValues(jAttribs.Hyperlink, a),
            ValueTypes.Number => BuildValues(jAttribs.Number, a),
            ValueTypes.String => BuildValues(jAttribs.String, a),
            ValueTypes.Custom => BuildValues(jAttribs.Custom, a),
            ValueTypes.Json => BuildValues(jAttribs.Json, a),
            // ReSharper disable RedundantCaseLabel
            ValueTypes.Empty or ValueTypes.Undefined =>
                // ReSharper restore RedundantCaseLabel
                new List<IValue>(),
            _ => throw new ArgumentOutOfRangeException()
        };

    private IList<IValue> BuildValues<T>(Dictionary<string, Dictionary<string, T>>? dic, IContentTypeAttribute attrDef)
    {
        if (dic == null || !dic.ContainsKey(attrDef.Name))
            return new List<IValue>();
        return dic[attrDef.Name]
            .Select(IValue (v) => Services.DataBuilder.Value.Create(v.Value, RecreateLanguageList(v.Key)))
            .ToListOpt();
    }

    private static IImmutableList<ILanguage> RecreateLanguageList(string languages) 
        => languages == NoLanguage
            ? DataConstants.NoLanguages
            : languages.Split(',')
                .Select(ILanguage (a) => new Language(a.Replace(ReadOnlyMarker, ""), a.StartsWith(ReadOnlyMarker)))
                .ToImmutableSafe();


    private Dictionary<string, Dictionary<string, string?>> ConvertReferences(Dictionary<string, Dictionary<string, string?>> links, Guid entityGuid)
    {
        var l = LogDsDetails.Fn<Dictionary<string, Dictionary<string, string?>>>();
        try
        {
            var converter = ((Dependencies)Services).ValueConverter.Value;
            var converted = links.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToDictionary(
                    val => val.Key,
                    val => converter.ToValue(val.Value, entityGuid)
                )
            );
            return l.ReturnAsOk(converted);
        }
        catch (Exception ex)
        {
            l.A("Ran into an error. Will log bug ignore and return original");
            l.Ex(ex);
            return l.Return(links, "error/ignored");
        }
    }

    /// <summary>
    /// Create a dictionary with each field, containing another dictionary with the language keys and the values of type T.
    /// </summary>
    /// <typeparam name="T">The underlying value in all these attributes, such as string, int or IEnumerable{IEntity}</typeparam>
    /// <param name="attribs"></param>
    /// <returns></returns>
    private Dictionary<string, Dictionary<string, T?>> ToTypedDictionary<T>(IEnumerable<IAttribute> attribs)
    {
        var l = LogDsDetails.Fn<Dictionary<string, Dictionary<string, T?>>>();
        var result = new Dictionary<string, Dictionary<string, T?>>();
        var attribsTyped = attribs
            .Cast<IAttribute<T>>()
            .ToListOpt();

        foreach (var a in attribsTyped)
        {

            Dictionary<string, T?> dimensions;
            try
            {
                dimensions = a.Typed.ToDictionary(LanguageKeyOfValue, v => v.TypedContents);
            }
            catch (Exception ex)
            {
                string? langList = null;
                try
                {
                    langList = string.Join(",", a.Typed.Select(LanguageKeyOfValue));
                }
                catch
                {
                    /* ignore */
                }

                l.W($"Error building languages list on '{a.Name}', probably multiple identical keys: {langList}");
                l.Done(ex);
                throw;
            }

            try
            {
                result.Add(a.Name, dimensions);
            }
            catch (Exception ex)
            {
                l.W($"Error adding attribute '{a.Name}' to dictionary, probably multiple identical keys");
                l.Done(ex);
                throw;
            }
        }

        return l.ReturnAsOk(result);
    }

    public IList<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false) 
        => serialized
            .Select(s => Deserialize(s, allowDynamic))
            .ToListOpt();
}