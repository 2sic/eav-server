using ToSic.Eav.Data.Entities.Sys;

namespace ToSic.Eav.Data.Build;

/// <summary>
/// Helper commands to build an entity
/// Should only be used by internal system data handlers - not for "public" use
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class EntityPreSave
{
    public static int? GetInternalPublishedIdForSaving(this Entity entity)
        => entity.PublishedEntityId;

    public static List<ILanguage> GetUsedLanguages(this IEntity entity)
        => entity.Attributes?.Values
            .SelectMany(v => v.Values)
            .SelectMany(vl => vl.Languages)
            .GroupBy(l => l.Key)
            .Select(g => g.First())
            .ToList() ?? DataConstants.NoLanguages.ToList();

}