using ToSic.Eav.DataSource.Internal.Catalog;
using ToSic.Eav.DataSource.VisualQuery.Internal;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;

namespace ToSic.Eav.DataSource.Internal.Query;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class QueryDefinitionBuilder: ServiceBase
{
    private readonly DataSourceCatalog _catalog;

    public QueryDefinitionBuilder(DataSourceCatalog catalog): base("Eav.QDefBl")
    {
        ConnectServices(
            _catalog = catalog
        );
    }

    public QueryDefinition Create(IEntity entity, int appId)
    {
        var parts = GenerateParts(entity);
        return new(entity, appId, parts, Log);
    }

    private List<QueryPartDefinition> GenerateParts(IEntity entity)
    {
        var l = Log.Fn<List<QueryPartDefinition>>();
        //Log.Add("Metadata Debug: " + (md as MetadataOf<Guid>)?.Debug());

        // Generate parts first
        var temp = entity.Metadata
            .Where(m => m.Type.Is(QueryConstants.QueryPartTypeName))
            .ToList();

        //Log.Add("Metadata Debug: " + (md as MetadataOf<Guid>)?.Debug());

        var parts = temp
            .Select(CreatePart)
            .ToList();
        return l.Return(parts, $"{parts.Count}");
    }

    public QueryPartDefinition CreatePart(IEntity entity)
    {
        var assemblyAndType = entity.GetBestValue<string>(QueryConstants.PartAssemblyAndType, null)
                              ?? throw new("Tried to get DataSource Type of a query part, but didn't find anything");

        var correctedName = GetCorrectedTypeName(assemblyAndType);
        var dsTypeIdentifier = _catalog.Find(correctedName, entity?.AppId ?? 0);
        var dsInfo = _catalog.FindDsiByGuidOrName(correctedName, entity?.AppId ?? 0)
                     ?? DataSourceInfo.CreateError(dsTypeIdentifier, false, DataSourceType.System,
                         new("Error finding data source", $"Tried to find {assemblyAndType} ({correctedName}) but can't find it."));

        return new(entity, dsTypeIdentifier, dsInfo.Type, dsInfo, Log);
    }


    /// <summary>
    /// Check if a Query part has an old assembly name, and if yes, correct it to the new name
    /// </summary>
    /// <returns></returns>
    private string GetCorrectedTypeName(string assemblyAndType)
    {
        // Correct old stored names (ca. before 2sxc 4 to new)
        var newName = assemblyAndType.EndsWith(V3To4DataSourceDllOld)
            ? assemblyAndType.Replace(V3To4DataSourceDllOld, V3To4DataSourceDllNew)
            : assemblyAndType;
        return newName;
    }

}