using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Core.Tests.Types
{
    public abstract class TypesBase // : ContentType
    {
        //protected const ValueTypes Str = ValueTypes.String;
        //protected const ValueTypes Bln = ValueTypes.Boolean;
        //protected const ValueTypes Num = ValueTypes.Number;
        //protected const ValueTypes Dtm = ValueTypes.DateTime;
        //protected const ValueTypes Grp = ValueTypes.Empty;
        //protected const ValueTypes Ent = ValueTypes.Entity;

        protected const string DefInp = "default";
        private const string UndefinedScope = "Undefined";

        // 2021-11-22 2dm disabled all of this, don't believe it's ever used, maybe #cleanup EOY 2021
        //protected TypesBase(string name, string staticName, string scope = UndefinedScope) : base(0, name, staticName)
        //{
        //    Scope = scope;
        //    Description = "todo";
        //    Attributes = new List<IContentTypeAttribute>();
        //    //IsInstalledInPrimaryStorage = false;
        //    RepositoryType = RepositoryTypes.Code;
        //    ParentId = Constants.PresetContentTypeFakeParent;  // important that parentid is different, so the GUI regards this as a ghost, and doesn't provide editing features
        //    //I18nKey = i18nKey;
        //}

        //protected ContentTypeAttribute Add(ContentTypeAttribute attDef)
        //{
        //    Attributes.Add(attDef);
        //    return attDef;
        //}

        //protected ContentTypeAttribute AttDef(ValueTypes type, string input, string name, string niceName = null, string description = null, string defaultValue = null)
        //{
        //    var correctedInput = type.ToString().ToLowerInvariant() + "-" + input;
        //    var attDef = new ContentTypeAttribute(AppId, name, /* niceName, */ type.ToString(), /* correctedInput, description, true, defaultValue, */
        //        attributeMetadata: new List<IEntity>
        //        {
        //            AttDefBuilder.GenerateAttributeMetadata(null /* TODO: NOW NEEDS GlobalTypes, but tests not updated */, AppId, niceName, description, true,
        //                HelpersToRefactor.SerializeValue(defaultValue), correctedInput)
        //        });
        //    return attDef;
        //}
    }
}
