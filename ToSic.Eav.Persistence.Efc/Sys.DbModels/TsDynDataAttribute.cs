﻿// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
#nullable disable // This is EFC code; values will be auto-generated on compile

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataAttribute
{
    public int AttributeId { get; set; }

    public string StaticName { get; set; }

    public string Type { get; set; }

    public Guid? Guid { get; set; }

    public string SysSettings { get; set; }

    public int ContentTypeId { get; set; }

    public int SortOrder { get; set; }

    public bool IsTitle { get; set; } = false;

    public int TransCreatedId { get; set; }

    public int? TransModifiedId { get; set; }

    public int? TransDeletedId { get; set; }

    public virtual TsDynDataAttributeType TypeNavigation { get; set; }

    public virtual TsDynDataContentType ContentType { get; set; }

    public virtual TsDynDataTransaction TransCreated { get; set; }

    public virtual TsDynDataTransaction TransModified { get; set; }

    public virtual TsDynDataTransaction TransDeleted { get; set; }

    public virtual ICollection<TsDynDataRelationship> TsDynDataRelationships { get; set; } = new HashSet<TsDynDataRelationship>();

    public virtual ICollection<TsDynDataValue> TsDynDataValues { get; set; } = new HashSet<TsDynDataValue>();
}