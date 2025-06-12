﻿using ToSic.Eav.Data.Dimensions.Sys;

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataDimension
{
    public int DimensionId { get; set; }

    public int? Parent { get; set; }

    public string Name { get; set; }

    public string Key { get; set; }

    public string EnvironmentKey { get; set; }

    public bool Active { get; set; }

    public int ZoneId { get; set; }

    public virtual ICollection<TsDynDataDimension> InverseParentNavigation { get; set; } = new HashSet<TsDynDataDimension>();

    public virtual TsDynDataDimension ParentNavigation { get; set; }

    public virtual ICollection<TsDynDataValueDimension> TsDynDataValueDimensions { get; set; } = new HashSet<TsDynDataValueDimension>();
    public virtual TsDynDataZone Zone { get; set; }
}