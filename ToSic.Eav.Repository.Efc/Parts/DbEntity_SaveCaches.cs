namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbEntity
{
    private (int ContentTypeId, List<ToSicEavAttributes> Attributes) GetContentTypeAndAttribIds(bool saveJson, IEntity newEnt, bool logDetails)
    {
        var l = Log.Fn<(int, List<ToSicEavAttributes>)>($"json: {saveJson}");
        if (saveJson)
            return l.Return((RepoIdForJsonEntities, null), $"json - no attributes, CT: {RepoIdForJsonEntities}");

        // 2023-02-28 2dm now #immutable, so the ID is not updated when a type was just imported
        // So if the TypeId is 0 (or anything invalid) it's a new type, and must be retrieved first
        var contentTypeId = newEnt.Type.Id;
        if (contentTypeId <= 0)
        {
            var typeNameId = newEnt.Type.NameId;
            if (!_ctNameIdCache.ContainsKey(typeNameId))
                _ctNameIdCache[typeNameId] = DbContext.AttribSet.GetId(typeNameId);
            contentTypeId = _ctNameIdCache[typeNameId];
        }

        if (logDetails)
            l.A($"Id on type: {newEnt.Type.Id}; NameId: {newEnt.Type.NameId}; Final ID: {contentTypeId}");

        if (!_ctCache.ContainsKey(contentTypeId))
            _ctCache[contentTypeId] = DbContext.Attributes.GetAttributeDefinitions(contentTypeId).ToList();
        var attributes = _ctCache[contentTypeId];

        if (logDetails)
            l.A(l.Try(() =>
                $"attribs: [{string.Join(",", attributes.Select(a => $"{a.AttributeId}:{a.StaticName}"))}]"));

        return l.Return((contentTypeId, attributes), $"{contentTypeId} / attribDefs⋮{attributes.Count}");
    }

    private readonly Dictionary<string, int> _ctNameIdCache = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<int, List<ToSicEavAttributes>> _ctCache = new Dictionary<int, List<ToSicEavAttributes>>();
    private void FlushTypeAttributesCache() => _ctCache.Clear();
}