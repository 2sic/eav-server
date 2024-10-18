namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAttributeSets
{
    public ToSicEavAttributeSets()
    {
        InverseUsesConfigurationOfAttributeSetNavigation = new HashSet<ToSicEavAttributeSets>();
        ToSicEavAttributesInSets = new HashSet<ToSicEavAttributesInSets>();
        ToSicEavEntities = new HashSet<ToSicEavEntities>();
        AlwaysShareConfiguration = false;
        //ToSicEavAttributesInSharedSets = new HashSet<ToSicEavAttributesInSets>();
        //ToSicEavAttributeGroups = new HashSet<ToSicEavAttributeGroups>();
    }

    public int AttributeSetId { get; set; }
    public string StaticName { get; set; }
    public string Name { get; set; }
    public string Scope { get; set; }
    public int ChangeLogCreated { get; set; }
    public int? ChangeLogDeleted { get; set; }
    public int AppId { get; set; }
    public int? UsesConfigurationOfAttributeSet { get; set; }
    public bool AlwaysShareConfiguration { get; set; }
    public string SysSettings { get; set; }

    public virtual ToSicEavApps App { get; set; }
    public virtual ToSicEavChangeLog ChangeLogCreatedNavigation { get; set; }
    public virtual ToSicEavChangeLog ChangeLogDeletedNavigation { get; set; }
    public virtual ToSicEavAttributeSets UsesConfigurationOfAttributeSetNavigation { get; set; }
    public virtual ICollection<ToSicEavAttributeSets> InverseUsesConfigurationOfAttributeSetNavigation { get; set; }
    public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; }
    public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; }
    //public virtual ICollection<ToSicEavAttributeGroups> ToSicEavAttributeGroups { get; set; }
    // 2dm added
    //public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSharedSets { get; set; }
}