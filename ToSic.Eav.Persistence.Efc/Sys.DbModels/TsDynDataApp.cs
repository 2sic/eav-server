// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataApp
{
    public int AppId { get; set; }

    public int ZoneId { get; set; }

    public required string Name { get; set; }

    public string? SysSettings { get; set; }

    public int? TransCreatedId { get; set; }

    public int? TransModifiedId { get; set; }

    public int? TransDeletedId { get; set; }

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypes { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntities { get; set; } = new HashSet<TsDynDataEntity>();

    public virtual TsDynDataZone Zone { get; set; } = null!;

    public virtual TsDynDataTransaction? TransCreated { get; set; }

    public virtual TsDynDataTransaction? TransModified { get; set; }

    public virtual TsDynDataTransaction? TransDeleted { get; set; }
}