namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavZones//: RepoZone
{
    public int ZoneId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<ToSicEavApps> ToSicEavApps { get; set; } = new HashSet<ToSicEavApps>();

    public virtual ICollection<ToSicEavDimensions> ToSicEavDimensions { get; set; } =  new HashSet<ToSicEavDimensions>();
}