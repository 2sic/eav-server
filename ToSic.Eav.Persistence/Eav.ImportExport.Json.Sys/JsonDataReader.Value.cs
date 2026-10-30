using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonDataReader
{
    private IList<IValue> GetValues(IContentTypeField a, JsonAttributes jAttribs, IEntitiesSource? relationshipsSource)
    {
        var relAssembler = dataAssembler.Relationship;
        return a.Type switch
        {
            ValueTypes.Boolean => BuildValues(jAttribs.Boolean, a),
            ValueTypes.DateTime => BuildValues(jAttribs.DateTime, a),
            ValueTypes.Entity => jAttribs.Entity == null || !jAttribs.Entity.ContainsKey(a.Name)
                ? new List<IValue>() // just keep the empty definition, as that's fine
                : jAttribs.Entity[a.Name]
                    .Select(v => relAssembler.Relationship(relAssembler.ToSource(v.Value, relationshipsSource ?? MyOptions.LazyRelationshipLookupList)))
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
    }

    private IList<IValue> BuildValues<T>(Dictionary<string, Dictionary<string, T>>? dic, IContentTypeField attrDef)
    {
        if (dic == null || !dic.ContainsKey(attrDef.Name))
            return new List<IValue>();
        return dic[attrDef.Name]
            .Select(IValue (v) => dataAssembler.Value.Create(v.Value, languages: RecreateLanguageList(v.Key)))
            .ToListOpt();
    }

}
