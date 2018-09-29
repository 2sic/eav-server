using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IContentTypeController
    {
        int AddField(int appId, int contentTypeId, string staticName, string type, string inputType, int sortOrder);
        bool CreateGhost(int appId, string sourceStaticName);
        string[] DataTypes(int appId);
        bool Delete(int appId, string staticName);
        bool DeleteField(int appId, int contentTypeId, int attributeId);
        IEnumerable<dynamic> Get(int appId, string scope = null, bool withStatistics = false);
        dynamic Get(int appId, string contentTypeId, string scope = null);
        IEnumerable<ContentTypeFieldInfo> GetFields(int appId, string staticName);
        dynamic GetSingle(int appId, string contentTypeStaticName, string scope = null);
        List<InputTypeInfo> InputTypes(int appId);
        void Rename(int appId, int contentTypeId, int attributeId, string newName);
        bool Reorder(int appId, int contentTypeId, string newSortOrder);
        bool Save(int appId, Dictionary<string, string> item);
        void SetTitle(int appId, int contentTypeId, int attributeId);
        bool UpdateInputType(int appId, int attributeId, string inputType);
    }
}