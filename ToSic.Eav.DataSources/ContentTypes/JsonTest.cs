using ToSic.Eav.Types;
using ToSic.Eav.Types.Attributes;
using ToSic.Eav.Types.Builder;

namespace ToSic.Eav.DataSources.ContentTypes
{
    [ContentTypeDefinition(StaticTypeName)]
    public class JsonTest: TypesBase
    {
        internal const string StaticTypeName = "x48d849d6-b83d-4001-96e5-79da0833e84e",
            NiceName = "JsonTest";

        public JsonTest() : base(NiceName, StaticTypeName, "json")
        {
            Add(AttDef(Str, DefInp, "Title", defaultValue:"from code").MakeTitle());
            Add(AttGrp("Group", "Group from Code"));
            {
                Add(AttDef(Str, DefInp, "Text", defaultValue: "from code"));
                Add(AttDef(Num, DefInp, "Number"));

                Add(AttDef(Ent, DefInp, "Entity1")).EntityDefault("BlogPost", false);
                Add(AttDef(Ent, DefInp, "EntityMulti")).EntityDefault("BlogPost", true);
            }

            Add(AttDef(Str, DefInp, "CodeText"));
        }
    }
}
