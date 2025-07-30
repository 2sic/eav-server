// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataTargetType
{
    public int TargetTypeId { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public virtual ICollection<TsDynDataEntity> TsDynDataEntities { get; set; } = new HashSet<TsDynDataEntity>();
}