using ToSic.Eav.Apps;
using ToSic.Eav.Data;

namespace ToSic.Eav.Repository.Efc.Tests
{
    public class SpecsDataEditing : IAppIdentity
    {
        public int ZoneId => 4;
        public int AppId => 3018;

        public string ContentTypeName = "DataToModify";

        public string TitleField = DataConstants.TitleField;

        public int ExistingItem = 20980;

    }
}
