// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataRelationship
{
    public int AttributeId { get; set; }

    public int ParentEntityId { get; set; }

    public int? ChildEntityId { get; set; }

    public Guid? ChildExternalId { get; set; }

    public int SortOrder { get; set; }

    public virtual TsDynDataAttribute Attribute { get; set; } = null!;

    public virtual TsDynDataEntity? ChildEntity { get; set; }

    public virtual TsDynDataEntity ParentEntity { get; set; } = null!;
}