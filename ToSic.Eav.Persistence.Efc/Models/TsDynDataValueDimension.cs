namespace ToSic.Eav.Persistence.Efc.Models;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataValueDimension
{
    public int ValueId { get; set; }

    public int DimensionId { get; set; }

    public bool ReadOnly { get; set; } = false;

    public virtual TsDynDataDimension Dimension { get; set; }

    public virtual TsDynDataValue Value { get; set; }
}