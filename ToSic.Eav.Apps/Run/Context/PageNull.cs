using System.Collections.Generic;

namespace ToSic.Eav.Apps.Run
{
    public class PageNull: IPage
    {
        public int Id => Constants.NullId;
        public string Url => null;
        public List<KeyValuePair<string, string>> Parameters => new List<KeyValuePair<string, string>>();
    }
}
