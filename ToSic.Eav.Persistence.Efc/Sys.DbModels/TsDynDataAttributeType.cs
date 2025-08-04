// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataAttributeType
{
    public required string Type { get; set; }

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributes { get; set; } = new HashSet<TsDynDataAttribute>();
}