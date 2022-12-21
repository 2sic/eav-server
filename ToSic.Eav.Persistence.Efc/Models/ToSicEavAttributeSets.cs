﻿using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavAttributeSets
    {
        public ToSicEavAttributeSets()
        {
            ToSicEavAttributeGroups = new HashSet<ToSicEavAttributeGroups>();
            ToSicEavAttributesInSets = new HashSet<ToSicEavAttributesInSets>();
            //ToSicEavAttributesInSharedSets = new HashSet<ToSicEavAttributesInSets>();
            ToSicEavEntities = new HashSet<ToSicEavEntities>();
        }

        public int AttributeSetId { get; set; }
        public string StaticName { get; set; }
        public string Name { get; set; }
        public string Scope { get; set; }
        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        public string Description { get; set; }
        public int ChangeLogCreated { get; set; }
        public int? ChangeLogDeleted { get; set; }
        public int AppId { get; set; }
        public int? UsesConfigurationOfAttributeSet { get; set; }
        public bool AlwaysShareConfiguration { get; set; }

        public virtual ICollection<ToSicEavAttributeGroups> ToSicEavAttributeGroups { get; set; }
        public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSets { get; set; }

        // 2dm added
        //public virtual ICollection<ToSicEavAttributesInSets> ToSicEavAttributesInSharedSets { get; set; }

        public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; }
        public virtual ToSicEavApps App { get; set; }
        public virtual ToSicEavChangeLog ChangeLogCreatedNavigation { get; set; }
        public virtual ToSicEavChangeLog ChangeLogDeletedNavigation { get; set; }
        public virtual ToSicEavAttributeSets UsesConfigurationOfAttributeSetNavigation { get; set; }
        public virtual ICollection<ToSicEavAttributeSets> InverseUsesConfigurationOfAttributeSetNavigation { get; set; }
    }
}
