namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class TsDynDataContentType
{
    public int ContentTypeId { get; set; }

    public string StaticName { get; set; }

    public string Name { get; set; }

    public string Scope { get; set; }

    public int TransactionIdCreated { get; set; }

    public int? TransactionIdModified { get; set; }

    public int? TransactionIdDeleted { get; set; }

    public int AppId { get; set; }

    public int? InheritContentTypeId { get; set; }

    public bool IsGlobal { get; set; } = false;

    // public string Json { get; set; }

    public string SysSettings { get; set; }

    public virtual TsDynDataApp App { get; set; }

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionModifiedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }

    public virtual TsDynDataContentType InheritContentTypeNavigation { get; set; }

    public virtual ICollection<TsDynDataContentType> InverseInheritContentTypesNavigation { get; set; } = new HashSet<TsDynDataContentType>();

    public virtual ICollection<TsDynDataAttribute> TsDynDataAttributes { get; set; } = new HashSet<TsDynDataAttribute>();

    public virtual ICollection<TsDynDataEntity> TsDynDataEntities { get; set; } = new HashSet<TsDynDataEntity>();



    // 2dm added
    //public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSharedSets { get; set; }
}