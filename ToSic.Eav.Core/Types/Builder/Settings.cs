using ToSic.Eav.Data;

namespace ToSic.Eav.Types.Builder
{
    public static class Settings
    {
        public static AttributeDefinition MakeTitle(this AttributeDefinition attDef)
        {
            attDef.IsTitle = true;
            return attDef;
        }
    }
}
