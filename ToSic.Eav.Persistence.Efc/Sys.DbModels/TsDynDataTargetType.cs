namespace ToSic.Eav.Persistence.Efc.Models;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataTargetType
{
    public int TargetTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public virtual ICollection<TsDynDataEntity> TsDynDataEntities { get; set; } = new HashSet<TsDynDataEntity>();
}