namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavApps
{
    public int AppId { get; set; }

    public int ZoneId { get; set; }

    public string Name { get; set; }

    public string SysSettings { get; set; } = null;

    public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSets { get; set; } = new HashSet<ToSicEavAttributeSets>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual TsDynDataZone Zone { get; set; }
}