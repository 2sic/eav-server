namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributeSets
{
    public int AttributeSetId { get; set; }

    public string StaticName { get; set; }

    public string Name { get; set; }

    public string Scope { get; set; }

    public int TransactionIdCreated { get; set; }

    public int? TransactionIdDeleted { get; set; }

    public int AppId { get; set; }

    public int? UsesConfigurationOfAttributeSet { get; set; }

    public bool AlwaysShareConfiguration { get; set; } = false;

    // public string Json { get; set; }

    public string SysSettings { get; set; }

    public virtual TsDynDataApp App { get; set; }

    public virtual TsDynDataTransaction TransactionCreatedNavigation { get; set; }

    public virtual TsDynDataTransaction TransactionDeletedNavigation { get; set; }

    public virtual ICollection<ToSicEavAttributeSets> InverseUsesConfigurationOfAttributeSetNavigation { get; set; } = new HashSet<ToSicEavAttributeSets>();

    public virtual ICollection<ToSicEavAttributes> ToSicEavAttributes { get; set; } = new HashSet<ToSicEavAttributes>();

    public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; } = new HashSet<ToSicEavEntities>();

    public virtual ToSicEavAttributeSets UsesConfigurationOfAttributeSetNavigation { get; set; }

    // 2dm added
    //public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSharedSets { get; set; }
}