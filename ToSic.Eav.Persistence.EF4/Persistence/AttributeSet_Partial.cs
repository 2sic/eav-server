using System;

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
    }
}
