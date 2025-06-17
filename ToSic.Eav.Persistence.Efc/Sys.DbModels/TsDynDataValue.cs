﻿// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
#nullable disable // This is EFC code; values will be auto-generated on compile

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataValue//: RepoValue
{
    public int ValueId { get; set; }

    public int EntityId { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; }

    public virtual TsDynDataAttribute Attribute { get; set; }

    public virtual TsDynDataEntity Entity { get; set; }


    public virtual ICollection<TsDynDataValueDimension> TsDynDataValueDimensions { get; set; } = new HashSet<TsDynDataValueDimension>();
}