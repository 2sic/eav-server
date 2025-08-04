﻿// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataContentType
{
    public int ContentTypeId { get; set; }

    public required string StaticName { get; set; }

    public required string Name { get; set; }

    public string? Scope { get; set; }

    public int TransCreatedId { get; set; }

    public int? TransModifiedId { get; set; }

    public int? TransDeletedId { get; set; }

    public int AppId { get; set; }

    public int? InheritContentTypeId { get; set; }

    public bool IsGlobal { get; set; } = false;

    public string? SysSettings { get; set; }

    public virtual TsDynDataApp App { get; set; } = null!;

    public virtual TsDynDataTransaction TransCreated { get; set; } = null!;

    public virtual TsDynDataTransaction TransModified { get; set; } = null!;

    public virtual TsDynDataTransaction? TransDeleted { get; set; }

    public virtual TsDynDataContentType InheritContentTypeNavigation { get; set; } = null!;

    public virtual ICollection<TsDynDataContentType> InverseInheritContentTypesNavigation { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributes { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntities { get; set; } = new HashSet<TsDynDataEntity>();

}