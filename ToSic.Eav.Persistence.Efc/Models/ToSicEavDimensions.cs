﻿namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavDimensions : Data.DimensionDefinition
{
    public ToSicEavDimensions()
    {
        Active = true;
    }

    //public int DimensionId { get; set; }

    //public int? Parent { get; set; }

    //public string Name { get; set; }

    //public string Key { get; set; }

    //public string EnvironmentKey { get; set; }

    //public bool Active { get; set; }

    public int ZoneId { get; set; }

    public virtual ICollection<ToSicEavDimensions> InverseParentNavigation { get; set; } = new HashSet<ToSicEavDimensions>();

    public virtual ToSicEavDimensions ParentNavigation { get; set; }

    public virtual ICollection<ToSicEavValuesDimensions> ToSicEavValuesDimensions { get; set; } = new HashSet<ToSicEavValuesDimensions>();
    public virtual ToSicEavZones Zone { get; set; }
}