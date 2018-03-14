using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Data;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.DataSources
{
    public sealed class App: Eav.DataSources.App, IAppData
    {
        public App(Log parentLog = null)
        {
            InitLog("DS.EavApp", parentLog);
        }

        internal string DefaultLanguage { get; set; }
        internal string CurrentUserName { get; set; }

        /// <summary>
        /// Get a correctly instantiated instance of the simple data controller.
        /// </summary>
        /// <returns>An data controller to create, update and delete entities</returns>
        private SimpleDataController DataController() => new SimpleDataController(ZoneId, AppId, DefaultLanguage, Log);

        public void Create(string contentTypeName,
            Dictionary<string, object> values, 
            string userName = null,
            MetadataFor target = null)
        {
            Log.Add($"app create new entity of type:{contentTypeName}");
            DataController().Create(contentTypeName, new List<Dictionary<string, object>> { values}, target); 
        }

        public void Create(string contentTypeName, 
            IEnumerable<Dictionary<string, object>> multiValues, 
            string userName = null)
        {
            Log.Add($"app create many ({multiValues.Count()}) new entities of type:{contentTypeName}");
            DataController().Create(contentTypeName, multiValues); 
        }

        public void Update(int entityId, Dictionary<string, object> values,
            string userName = null)
        {
            Log.Add($"app update i:{entityId}");
            DataController().Update(entityId, values);
        }


        public void Delete(int entityId, string userName = null)
        {
            Log.Add($"app delete i:{entityId}");
            DataController().Delete(entityId);
        }
 
    }
}