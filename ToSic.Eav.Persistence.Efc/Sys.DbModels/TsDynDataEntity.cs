// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataEntity
{
    public int EntityId { get; set; }

    public Guid EntityGuid { get; set; }

    public int ContentTypeId { get; set; }

    public int TargetTypeId { get; set; }

    public int? KeyNumber { get; set; }

    public Guid? KeyGuid { get; set; }

    public string? KeyString { get; set; }

    public bool IsPublished { get; set; }

    public int? PublishedEntityId { get; set; }

    public required string Owner { get; set; }

    public string? Json { get; set; }

    public int Version { get; set; } = 1;

    public int AppId { get; set; }

    public string? ContentType { get; set; }

    public int TransCreatedId { get; set; }

    public int TransModifiedId { get; set; }

    public int? TransDeletedId { get; set; }

    // 2017-10-10 2dm new with entity > app mapping
    public virtual TsDynDataApp App { get; set; } = null!;

    public virtual TsDynDataTargetType TargetType { get; set; } = null!;

    public virtual TsDynDataContentType ContentTypeNavigation { get; set; } = null!;

    public virtual TsDynDataTransaction TransCreated { get; set; } = null!;

    public virtual TsDynDataTransaction TransModified { get; set; } = null!;

    public virtual TsDynDataTransaction? TransDeleted { get; set; }

    public virtual ICollection<TsDynDataRelationship> RelationshipsWithThisAsChild { get; set; } = new HashSet<TsDynDataRelationship>();

    public virtual ICollection<TsDynDataRelationship> RelationshipsWithThisAsParent { get; set; } = new HashSet<TsDynDataRelationship>();

    public virtual ICollection<TsDynDataValue> TsDynDataValues { get; set; } = new HashSet<TsDynDataValue>();
}