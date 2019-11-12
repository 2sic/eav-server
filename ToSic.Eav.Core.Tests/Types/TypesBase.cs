using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Core.Tests.Types
{
    public abstract class TypesBase: ContentType
    {
        protected const ValueTypes Str = ValueTypes.String;
        protected const ValueTypes Bln = ValueTypes.Boolean;
        protected const ValueTypes Num = ValueTypes.Number;
        protected const ValueTypes Dtm = ValueTypes.DateTime;
        protected const ValueTypes Grp = ValueTypes.Empty;
        protected const ValueTypes Ent = ValueTypes.Entity;

        protected const string DefInp = "default";
        public const string UndefinedScope = "Undefined";

        protected TypesBase(string name, string staticName, string scope = UndefinedScope, string i18nKey = null) : base(0, name, staticName)
        {
            Scope = scope;
            Description = "todo";
            Attributes = new List<IAttributeDefinition>();
            //IsInstalledInPrimaryStorage = false;
            RepositoryType = RepositoryTypes.Code;
            ParentId = Constants.SystemContentTypeFakeParent;  // important that parentid is different, so the GUI regards this as a ghost, and doesn't provide editing features
            I18nKey = i18nKey;
        }

        protected AttributeDefinition Add(AttributeDefinition attDef)
        {
            Attributes.Add(attDef);
            return attDef;
        }

        protected AttributeDefinition AttDef(ValueTypes type, string input, string name, string niceName = null, string description = null, string defaultValue = null)
        {
            var correctedInput = type.ToString().ToLowerInvariant() + "-" + input;
            var attDef = new AttributeDefinition(AppId, name, niceName, type.ToString(), correctedInput, description, true, defaultValue);
            return attDef;
        }
    }
}
