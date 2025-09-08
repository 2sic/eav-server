// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataZone//: RepoZone
{
    public int ZoneId { get; set; }

    public int? TenantId { get; set; }

    public int? SiteId { get; set; }

    public required string Name { get; set; }

    public string? AppBasePath { get; set; }

    public string? AppBaseSharedPath { get; set; }

    public int? TransCreatedId { get; set; }

    public int? TransModifiedId { get; set; }

    public int? TransDeletedId { get; set; }

    public virtual ICollection<TsDynDataApp> TsDynDataApps { get; set; } = new HashSet<TsDynDataApp>();

    public virtual ICollection<TsDynDataDimension> TsDynDataDimensions { get; set; } =  new HashSet<TsDynDataDimension>();

    public virtual TsDynDataTransaction TransCreated { get; set; } = null!;

    public virtual TsDynDataTransaction TransModified { get; set; } = null!;

    public virtual TsDynDataTransaction? TransDeleted { get; set; }
}