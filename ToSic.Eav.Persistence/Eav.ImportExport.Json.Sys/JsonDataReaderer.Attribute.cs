using System.Collections.Immutable;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Json.V1;
using static System.StringComparer;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonDataReader
{


    private IReadOnlyDictionary<string, IAttribute> BuildAttribsOfUnknownContentType(JsonAttributes jAttributes, IEntitiesSource? relationshipsSource = null)
    {
        var l = LogDsDetails.Fn<IReadOnlyDictionary<string, IAttribute>>(timer: true);
        var valBuilder = dataAssembler.Value;
        var relBuilder = dataAssembler.Relationship;
        var attribs = new[]
        {
            BuildAttrib(jAttributes.DateTime, ValueTypes.DateTime, valBuilder.DateTime),
            BuildAttrib(jAttributes.Boolean, ValueTypes.Boolean, valBuilder.Bool),
            BuildAttrib(jAttributes.Custom, ValueTypes.Custom, valBuilder.String),
            BuildAttrib(jAttributes.Json, ValueTypes.Json, valBuilder.String),
            BuildAttrib(jAttributes.Entity, ValueTypes.Entity, (v, _) => relBuilder.Relationship(relBuilder.ToSource(v, relationshipsSource))),
            BuildAttrib(jAttributes.Hyperlink, ValueTypes.Hyperlink, valBuilder.String),
            BuildAttrib(jAttributes.Number, ValueTypes.Number, valBuilder.Number),
            BuildAttrib(jAttributes.String, ValueTypes.String, valBuilder.String)
        };
        var final = attribs
            .Where(dic => dic != null)
            .SelectMany(dic => dic!)
            .ToImmutableDicSafe(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);

        return l.ReturnAsOk(final);

        Dictionary<string, IAttribute>? BuildAttrib<T>(Dictionary<string, Dictionary<string, T>>? list, ValueTypes type, Func<T, IImmutableList<ILanguage>, IValue> valueBuilder)
        {
            if (list == null)
                return null;

            var builder = dataAssembler;
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
                    return dataAssembler.Attribute.Create(a.Name, a.Type, values);
                },
                InvariantCultureIgnoreCase
            );
        return l.ReturnAsOk(result);
    }

}
