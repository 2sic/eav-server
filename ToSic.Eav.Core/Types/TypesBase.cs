using ToSic.Eav.Data;
using ToSic.Eav.Enums;

namespace ToSic.Eav.Types
{
    public abstract class TypesBase: ContentType
    {
        protected const AttributeTypeEnum Str = AttributeTypeEnum.String;
        protected const AttributeTypeEnum Bln = AttributeTypeEnum.Boolean;
        protected const AttributeTypeEnum Num = AttributeTypeEnum.Number;
        protected const AttributeTypeEnum Dtm = AttributeTypeEnum.DateTime;
        protected const AttributeTypeEnum Grp = AttributeTypeEnum.Empty;
        protected const AttributeTypeEnum Ent = AttributeTypeEnum.Entity;

        protected const string DefInp = "default";

        protected TypesBase(string name, string staticName) : base(0, name, staticName)
        {
            Scope = "assembly";
            Description = "todo";
        }

        protected AttributeDefinition Add(AttributeDefinition attDef)
        {
            Attributes.Add(attDef);
            return attDef;
        }

        protected AttributeDefinition AttDef(AttributeTypeEnum type, string input, string name, string niceName, string description, string defaultValue = null)
        {
            var correctedInput = "@" + type.ToString().ToLowerInvariant() + "-" + input;
            var attDef = new AttributeDefinition(AppId, name, niceName, type, correctedInput, description, true, defaultValue);
            return attDef;
        }
    }
}
