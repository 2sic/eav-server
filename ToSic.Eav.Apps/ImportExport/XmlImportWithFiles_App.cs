using System;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {
        /// <summary>
        /// Creates an app and then imports the xml
        /// </summary>
        /// <returns>AppId of the new imported app</returns>
        public bool ImportApp(int zoneId, XDocument doc, out int appId)
        {
            var wrapLog = Log.Call<bool>($"zone:{zoneId}");

            appId = 0;

			if (!IsCompatible(doc))
			{
				Messages.Add(new Message(Log.Add("The import file is not compatible with the installed version of 2sxc."), Message.MessageTypes.Error));
				return false;
			}

			// Get root node "SexyContent"
			var xmlSource = doc.Element(XmlConstants.RootNode);
			var xApp = xmlSource?.Element(XmlConstants.Header)?.Element(XmlConstants.App);

			var appGuid = xApp?.Attribute(XmlConstants.Guid)?.Value;

            if (appGuid == null)
            {
                Messages.Add(new Message(Log.Add("Something is wrong in the xml structure, can't get an app-guid"), Message.MessageTypes.Error));
                return false;
            }

            if (appGuid != XmlConstants.AppContentGuid)
            {
                // Build Guid (take existing, or create a new)
                if (string.IsNullOrEmpty(appGuid) || appGuid == new Guid().ToString())
                    appGuid = Guid.NewGuid().ToString();

                // Adding app to EAV
                var eavDc = DbDataController.Instance(zoneId, null, Log);
                var app = eavDc.App.AddApp(null, appGuid);
                eavDc.SqlDb.SaveChanges();

                appId = app.AppId;
            }
            else
                appId = AppId;

            if (appId <= 0)
			{
				Messages.Add(new Message(Log.Add("App was not created. Please try again or make sure the package you are importing is correct."), Message.MessageTypes.Error));
				return false;
			}

            Log.Add("Purging all Zones");
            State.Cache.PurgeZones();
            return wrapLog("done", ImportXml(zoneId, appId, doc));
		}


	}

}