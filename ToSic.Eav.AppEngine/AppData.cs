using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The Data object on an App. It's also a data-source of type <see cref="Eav.DataSources.App"/>,
    /// so it has many streams, one for each content-type so you can use it in combination with other DataSources. <br/>
    /// The special feature is that it also has methods for data-manipulation,
    /// including Create, Update and Delete
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public sealed class AppData: Eav.DataSources.App, IAppData
    {
        [PrivateApi]
        public AppData(ILog parentLog = null)
        {
            InitLog("DS.EavApp", parentLog);
        }

        [PrivateApi]
        internal string DefaultLanguage { get; set; }
        [PrivateApi]
        internal string CurrentUserName { get; set; }

        /// <summary>
        /// Get a correctly instantiated instance of the simple data controller.
        /// </summary>
        /// <returns>An data controller to create, update and delete entities</returns>
        private SimpleDataController DataController() => new SimpleDataController(ZoneId, AppId, DefaultLanguage, Log);

        /// <inheritdoc />
        public void Create(string contentTypeName,
            Dictionary<string, object> values, 
            string userName = null,
            ITarget target = null)
        {
            Log.Add($"app create new entity of type:{contentTypeName}");
            DataController().Create(contentTypeName, new List<Dictionary<string, object>> {values}, target);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            RequiresRebuildOfOut = true;
        }

        /// <inheritdoc />
        public void Create(string contentTypeName, 
            IEnumerable<Dictionary<string, object>> multiValues, 
            string userName = null)
        {
            Log.Add($"app create many ({multiValues.Count()}) new entities of type:{contentTypeName}");
            DataController().Create(contentTypeName, multiValues);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            RequiresRebuildOfOut = true;
        }

        /// <inheritdoc />
        public void Update(int entityId, Dictionary<string, object> values,
            string userName = null)
        {
            Log.Add($"app update i:{entityId}");
            DataController().Update(entityId, values);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            RequiresRebuildOfOut = true;
        }


        /// <inheritdoc />
        public void Delete(int entityId, string userName = null)
        {
            Log.Add($"app delete i:{entityId}");
            DataController().Delete(entityId);
            // Out must now be rebuilt, because otherwise it will still have old data in the streams
            RequiresRebuildOfOut = true;
        }

    }
}