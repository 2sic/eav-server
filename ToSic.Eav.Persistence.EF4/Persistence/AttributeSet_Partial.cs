using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav
{
    public partial class AttributeSet
    {
        // Ensure Static name of new AttributeSets
        public AttributeSet()
        {
            _StaticName = Guid.NewGuid().ToString();
            _AppID = AppID;
        }


        public IEnumerable<Attribute> GetAttributes() 
            => AttributesInSets.Select(item => item.Attribute).ToList();

        public Attribute AttributeByName(string attributeName) 
            => GetAttributes().FirstOrDefault(attr => attr.StaticName == attributeName);

        public Entity EntityByGuid( Guid entityGuid) 
            => Entities.FirstOrDefault(entity => entity.EntityGUID == entityGuid && entity.ChangeLogIDDeleted == null);
    }
}
