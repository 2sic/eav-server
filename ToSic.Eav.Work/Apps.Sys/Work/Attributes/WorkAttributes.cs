using ToSic.Eav.Data.Shared;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkAttributes() : WorkUnitBase<IAppWorkCtx>("Wrk.Attrib")
{
    public List<PairTypeWithAttribute> GetFields(string staticName)
    {
        var l = Log.Fn<List<PairTypeWithAttribute>>($"a#{AppWorkCtx.Show()}, type:{staticName}");

        if (AppWorkCtx.AppReader.GetContentType(staticName) is not { } type)
            return l.Return([], $"error, type:{staticName} is null. Missing or not a ContentType.");

        var fields = type.Attributes.OrderBy(a => a.SortOrder);

        return l.Return(fields.Select(a => new PairTypeWithAttribute { Type = type, Attribute = a }).ToList());
    }

    /// <summary>
    /// Get shared fields, 2 scenarios
    /// 1. either all shared fields of the current App
    /// 2. or just the shared fields which match an attribute (to be able to assign it)
    /// </summary>
    /// <param name="attributeId">optional, if 0, will return all shared fields of all types</param>
    /// <returns></returns>
    public List<PairTypeWithAttribute> GetSharedFields(int attributeId = 0)
    {
        var l = Log.Fn<List<PairTypeWithAttribute>>($"get shared fields {AppWorkCtx.Show()}");

        // If we know what AttributeId it's for, then filter
        var attributeType = ValueTypes.Undefined;
        if (attributeId != default)
        {
            var attribute = GetAttribute(attributeId);
            if (attribute == null)
                return l.Return([], $"attribute {attributeId} not found");
            attributeType = attribute.Type;
        }

        var fields = GetLocalTypes()
            .SelectMany(ct => ct.Attributes
                .Where(a =>
                    a.AttributeId != 0 // don't take AttributeId 0 which would be file-system based (not in DB)
                    && a.Guid != null // Guid required for sharing
                    && a.SysSettings?.Share == true // Share must be defined
                    && (attributeType == ValueTypes.Undefined || a.Type == attributeType)
                )
                .Select(a => new PairTypeWithAttribute { Type = ct, Attribute = a }))
            .OrderBy(set => set.Type.Name)
            .ThenBy(set => set.Attribute.Name)
            .ToList();

        return l.Return(fields);
    }

    public List<PairTypeWithAttribute> GetAncestors(int attributeId)
    {
        var l = Log.Fn<List<PairTypeWithAttribute>>($"attribute: {attributeId}");
        var attribute = GetAttribute(attributeId);
        if (attribute == null)
            return l.Return([], $"attribute {attributeId} not found");

        if (attribute.SysSettings?.InheritMetadata != true)
            return l.Return([], $"attribute {attributeId} does not inherit metadata");

        var sources = attribute.SysSettings.InheritMetadataOf;
        var allShared = GetSharedFields();

        var result = sources
            .Select(pair => allShared.FirstOrDefault(a => a.Attribute.Guid == pair.Key))
            .Where(x => x != null)
            .ToList();
        
        return l.Return(result, $"{result.Count}");
    }

    public List<PairTypeWithAttribute> GetDescendants(int attributeId)
    {
        var l = Log.Fn<List<PairTypeWithAttribute>>($"attribute: {attributeId}");
        var attribute = GetAttribute(attributeId);
        if (attribute == null)
            return l.Return([], $"attribute {attributeId} not found");

        if (attribute.SysSettings?.Share != true)
            return l.Return([], $"attribute {attributeId} does not share");

        if (attribute.Guid == null)
            return l.Return([], $"attribute {attributeId} does not have a GUID to share");

        var guid = attribute.Guid.Value;

        var allShared = GetLocalTypes()
            .SelectMany(ct => ct.Attributes
                .Where(a => a.SysSettings?.InheritMetadata == true)
                .Select(a => new PairTypeWithAttribute { Type = ct, Attribute = a }))
            .ToList();

        var result = allShared
            .Where(a => a.Attribute.SysSettings.InheritMetadataOf.ContainsKey(guid))
            .ToList();

        return l.Return(result, $"{result.Count}");
    }

    private List<IContentType> GetLocalTypes() =>
        AppWorkCtx.AppReader.ContentTypes
            .Where(ct => !ct.HasAncestor())
            .ToList();

    private IContentTypeAttribute GetAttribute(int attributeId) =>
        AppWorkCtx.AppReader.ContentTypes
            .SelectMany(ct => ct.Attributes.Where(a => a.AttributeId == attributeId))
            .FirstOrDefault();
}