namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavApps
{
    public ToSicEavApps()
    {
        ToSicEavAttributeSets = new HashSet<ToSicEavAttributeSets>();

        // 2017-10-10 2dm new with entity > app mapping
        ToSicEavEntities = new HashSet<ToSicEavEntities>();
    }

    public int AppId { get; set; }
    public int ZoneId { get; set; }
    public string Name { get; set; }

    public virtual ICollection<ToSicEavAttributeSets> ToSicEavAttributeSets { get; set; }
    public virtual ToSicEavZones Zone { get; set; }

    // 2017-10-10 2dm new with entity > app mapping
    public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; }

    /// <summary>
    /// Special column to store info about loading the app
    /// </summary>
    /// <remarks>Added v13.01</remarks>
    public string SysSettings { get; set; } = null;
}