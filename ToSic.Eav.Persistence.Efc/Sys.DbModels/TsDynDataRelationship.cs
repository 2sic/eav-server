﻿// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
#nullable disable // This is EFC code; values will be auto-generated on compile

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataRelationship
{
    public int AttributeId { get; set; }

    public int ParentEntityId { get; set; }

    public int? ChildEntityId { get; set; }

    public int SortOrder { get; set; }

    public virtual TsDynDataAttribute Attribute { get; set; }

    public virtual TsDynDataEntity ChildEntity { get; set; }

    public virtual TsDynDataEntity ParentEntity { get; set; }
}