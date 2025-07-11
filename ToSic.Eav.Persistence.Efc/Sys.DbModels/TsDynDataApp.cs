﻿// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
#nullable disable // This is EFC code; values will be auto-generated on compile

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataApp
{
    public int AppId { get; set; }

    public int ZoneId { get; set; }

    public string Name { get; set; }

    public string SysSettings { get; set; } = null;

    public int? TransCreatedId { get; set; }

    public int? TransModifiedId { get; set; }

    public int? TransDeletedId { get; set; }

    public virtual ICollection<TsDynDataContentType> TsDynDataContentTypes { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntities { get; set; } = new HashSet<TsDynDataEntity>();

    public virtual TsDynDataZone Zone { get; set; }

    public virtual TsDynDataTransaction TransCreated { get; set; }

    public virtual TsDynDataTransaction TransModified { get; set; }

    public virtual TsDynDataTransaction TransDeleted { get; set; }
}