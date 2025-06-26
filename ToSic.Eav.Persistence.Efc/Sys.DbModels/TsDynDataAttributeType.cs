// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
#nullable disable // This is EFC code; values will be auto-generated on compile

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataAttributeType
{
    public string Type { get; set; }

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributes { get; set; } = new HashSet<TsDynDataAttribute>();
}