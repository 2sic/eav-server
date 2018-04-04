// 2018-03-09 2dm - this was used when we tried creating code-based content-types, but I believe it's dead code now
//using System.Collections.Generic;
//using ToSic.Eav.Data;

//namespace ToSic.Eav.Types.Builder
//{
//    public static class Entity
//    {
//        public static AttributeDefinition EntityDefault(this AttributeDefinition attDef, string entityType, 
//            bool allowMulti = false, bool enableAdd = true, bool enableCreate = true, bool enableEdit = true, bool enableRemove = true, bool enableDelete = false)
//        {
//            attDef.Metadata.Add("@entity-default", new Dictionary<string, object>
//            {
//                {"EntityType", entityType},
//                {"AllowMultiValue", allowMulti},
//                {"EnableCreate", enableCreate},
//                {"EnableEdit", enableEdit},
//                {"EnableAddExisting", enableAdd},
//                {"EnableRemove", enableRemove},
//                {"EnableDelete", enableDelete}
//            });
//            return attDef; // for chaining...
//        }
//    }
//}
