using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSource.VisualQuery.Sys;

namespace ToSic.Eav.DataSource.Query.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryDefinitionBuilder(DataSourceCatalog catalog) : ServiceBase("Eav.QDefBl", connect: [catalog])
{
    public QueryDefinition Create(IEntity entity, int appId)
    {
        var parts = GenerateParts(entity);
        return new(entity, appId, parts);
    }

    private List<QueryPartDefinition> GenerateParts(IEntity entity)
    {
        var l = Log.Fn<List<QueryPartDefinition>>();

        // Generate parts first
        var temp = entity.Metadata
            .Where(m => m.Type.Is(QueryPartDefinition.TypeName))
            .ToList();

        var parts = temp
            .Select(CreatePart)
            .ToList();
        return l.Return(parts, $"{parts.Count}");
    }

    public QueryPartDefinition CreatePart(IEntity entity)
    {
        var assemblyAndType = entity.Get<string>(nameof(QueryPartDefinition.PartAssemblyAndType))
                              ?? throw new("Tried to get DataSource Type of a query part, but didn't find anything");

        var correctedName = GetCorrectedTypeName(assemblyAndType);
        var dsTypeIdentifier = catalog.Find(correctedName, entity.AppId);
        var dsInfo = catalog.FindDsiByGuidOrName(correctedName, entity.AppId)
                     ?? DataSourceInfo.CreateError(dsTypeIdentifier, false, DataSourceType.System,
                         new("Error finding data source", $"Tried to find {assemblyAndType} ({correctedName}) but can't find it."));

        return new(entity, dsTypeIdentifier, dsInfo.Type, dsInfo);
    }


    /// <summary>
    /// Check if a Query part has an old assembly name, and if yes, correct it to the new name
    /// </summary>
    /// <returns></returns>
    private string GetCorrectedTypeName(string assemblyAndType)
    {
        // Correct old stored names (ca. before 2sxc 4 to new)
        var newName = assemblyAndType.EndsWith(DataSourceConstantsInternal.V3To4DataSourceDllOld)
            ? assemblyAndType.Replace(DataSourceConstantsInternal.V3To4DataSourceDllOld, DataSourceConstantsInternal.V3To4DataSourceDllNew)
            : assemblyAndType;
        return newName;
    }

}