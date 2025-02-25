﻿namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavValuesDimensions
{
    public int ValueId { get; set; }

    public int DimensionId { get; set; }

    public bool ReadOnly { get; set; } = false;

    public virtual ToSicEavDimensions Dimension { get; set; }

    public virtual ToSicEavValues Value { get; set; }
}