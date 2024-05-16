namespace ToSic.Eav.Data.Build;

/// <summary>
/// Helper commands to build an entity
/// Should only be used by internal system data handlers - not for "public" use
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class EntityPreSave
{


    public static int? GetInternalPublishedIdForSaving(this Entity entity) => entity.PublishedEntityId;
        
    public static List<ILanguage> GetUsedLanguages(this IEntity entity) => entity.Attributes?.Values
        .SelectMany(v => v.Values)
        .SelectMany(vl => vl.Languages)
        .GroupBy(l => l.Key)
        .Select(g => g.First())
        .ToList() ?? DimensionBuilder.NoLanguages.ToList();

}