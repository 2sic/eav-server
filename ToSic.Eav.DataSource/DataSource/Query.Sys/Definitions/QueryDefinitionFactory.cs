using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSource.VisualQuery.Sys;

namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Helper to build a <see cref="QueryDefinition"/> objects based on a query definition entity.
/// </summary>
/// <remarks>
/// Made visible in the docs v21.02, but still just fyi/internal.
/// </remarks>
/// <param name="dsCatalog"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class QueryDefinitionFactory(DataSourceCatalog dsCatalog) : ServiceBase("Eav.QDefBl", connect: [dsCatalog])
{
    /// <summary>
    /// Create a <see cref="QueryDefinition"/> based on the given query definition entity.
    /// </summary>
    /// <remarks>
    ///  The entity should have the correct metadata and properties to be used as a query definition, otherwise this will fail.
    /// This is usually used to create a QueryDefinition from an entity stored in the database, but it can also be used to create a QueryDefinition from an entity created in memory (e.g. for testing purposes).
    /// </remarks>
    /// <param name="appId"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public QueryDefinition Create(int appId, IEntity entity)
    {
        var parts = GenerateParts(entity);
        return new(entity, appId, parts);
    }

    private List<QueryPartDefinition> GenerateParts(IEntity entity)
    {
        var l = Log.Fn<List<QueryPartDefinition>>();

        var partEntities = entity.Metadata
            .Where(m => m.Type.Is(QueryPartDefinition.TypeName))
            .ToList();

        var parts = partEntities
            .Select(CreatePart)
            .ToList();

        return l.Return(parts, $"{parts.Count}");
    }

    private QueryPartDefinition CreatePart(IEntity entity)
    {
        var assemblyAndType = entity.Get<string>(nameof(QueryPartDefinition.PartAssemblyAndType))
                              ?? throw new("Tried to get DataSource Type of a query part, but didn't find anything");

        var correctedName = GetCorrectedTypeName(assemblyAndType);
        var dsTypeIdentifier = dsCatalog.Find(correctedName, entity.AppId);
        var dsInfo = dsCatalog.FindDsiByGuidOrName(correctedName, entity.AppId)
                     ?? DataSourceInfo.CreateError(dsTypeIdentifier, false, DataSourceType.System,
                         new("Error finding data source", $"Tried to find {assemblyAndType} ({correctedName}) but can't find it."));

        return new(entity, dsTypeIdentifier, dsInfo.Type, dsInfo);
    }


    /// <summary>
    /// Check if a Query part has an old assembly name, and if yes, correct it to the new name
    /// </summary>
    /// <returns></returns>
    private static string GetCorrectedTypeName(string assemblyAndType)
    {
        // Correct old stored names (ca. before 2sxc 4 to new)
        var newName = assemblyAndType.EndsWith(DataSourceConstantsInternal.V3To4DataSourceDllOld)
            ? assemblyAndType.Replace(DataSourceConstantsInternal.V3To4DataSourceDllOld, DataSourceConstantsInternal.V3To4DataSourceDllNew)
            : assemblyAndType;
        return newName;
    }

}