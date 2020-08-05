using System;
using System.Text;

namespace ToSic.Eav.WebApi.Dto
{
    public class EntityImportDto
    {
        public int AppId;

        public string ContentBase64;

        public string DebugInfo => $"app:{AppId} + base:{ContentBase64}";

        public string GetContentString()
        {
            var data = Convert.FromBase64String(ContentBase64);
            var str = Encoding.UTF8.GetString(data);
            return str;
        }
    }
}
