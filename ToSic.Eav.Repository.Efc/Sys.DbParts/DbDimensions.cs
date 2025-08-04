using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbDimensions(DbStorage.DbStorage db) : DbPartBase(db, "Db.Dims")
{
    private int GetDimensionId(string systemKey, string? externalKey)
    {
        // Because of changes in EF 3.x we had to split where part on server and client.
        return DbStore.SqlDb
            .TsDynDataDimensions
            .AsNoTracking()
            .Where(d => d.ZoneId == DbStore.ZoneId) // This is evaluated on the SQL server
            .ToList()
            .Where(d =>
                string.Equals(d.EnvironmentKey, externalKey, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(d.Key, systemKey, StringComparison.InvariantCultureIgnoreCase)) // This is evaluated on the client
            .Select(d => d.DimensionId)
            .FirstOrDefault();
    }

    /// <summary>
    /// Update a single Dimension
    /// </summary>
    private void UpdateZoneDimension(int dimensionId, bool? active = null, string? name = null)
        => DbStore.DoAndSaveTracked(() =>
        {
            var dimension = DbStore.SqlDb
                .TsDynDataDimensions
                .Single(d => d.DimensionId == dimensionId);

            if (active.HasValue)
                dimension.Active = active.Value;
            if (name != null)
                dimension.Name = name;
        });

    /// <summary>
    /// Add or update a language. Must use this kind of logic because the client doesn't know if a language
    /// is missing, or has been disabled
    /// </summary>
    /// <param name="cultureCode"></param>
    /// <param name="cultureText"></param>
    /// <param name="active"></param>
    internal void AddOrUpdateZoneDimension(string cultureCode, string cultureText, bool active)
    {
        var eavLanguage = GetLanguagesUntracked(true)
            .FirstOrDefault(l => l.Matches(cultureCode));
        // If the language exists in EAV, set the active state, else add it
        if (eavLanguage != null)
            UpdateZoneDimension(eavLanguage.DimensionId, active);
        else
            AddZoneDimension(cultureText, cultureCode);
    }


    /// <summary>
    /// Add a new Dimension at the top of the dimension tree.
    /// This is used by the "create new zone" code
    /// </summary>
    internal void AddRootCultureNode(string systemKey, string name, TsDynDataZone zone)
        => DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.Add(new TsDynDataDimension
        {
            Key = systemKey,
            Name = name,
            Zone = zone,
            ParentNavigation = null,
            Active = true,
        }));

    #region Languages

    /// <summary>
    /// Get all Languages of current Zone and App
    /// </summary>
    private ICollection<DimensionDefinition> GetLanguagesUntracked(bool includeInactive = false)
        => DbStore.SqlDb.TsDynDataDimensions
            .AsNoTracking()
            .Include(d => d.ParentNavigation)
            .Where(d => d.ZoneId == DbStore.ZoneId) // This is evaluated on the SQL server
            .ToList()
            .Where(d =>
                d.Parent.HasValue
                && d.ParentNavigation!.Key == EavConstants.CultureSystemKey
                && (includeInactive || d.Active)
            )
            .Select(d => d.AsDimensionDefinition)
            .ToListOpt();

    /// <summary>
    /// Add a new Language to current Zone
    /// </summary>
    private void AddZoneDimension(string name, string externalKey)
        => DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.Add(new TsDynDataDimension
        {
            Name = name,
            EnvironmentKey = externalKey,
            Parent = GetDimensionId(EavConstants.CultureSystemKey, null),
            ZoneId = DbStore.ZoneId,
            Active = true
        }));

    #endregion
}