﻿namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributeTypes
{
    public string Type { get; set; }

    public virtual ICollection<ToSicEavAttributes> ToSicEavAttributes { get; set; } = new HashSet<ToSicEavAttributes>();
}