using System;
using System.Xml.Linq;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Lib.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Internal;

partial class XmlImportWithFiles
{
    /// <summary>
    /// Creates an app and then imports the xml
    /// </summary>
    /// <returns>AppId of the new imported app</returns>
    public bool ImportApp(int zoneId, XDocument doc, out int appId)
    {
        var wrapLog = Log.Fn<bool>($"zone:{zoneId}");

        appId = 0;
        int? parentAppId = null;

        if (!IsCompatible(doc))
            return wrapLog.ReturnFalse(LogError("The import file is not compatible with the installed version of 2sxc."));

        // Get root node "SexyContent"
        var xmlSource = doc.Element(XmlConstants.RootNode);
        var xApp = xmlSource?.Element(XmlConstants.Header)?.Element(XmlConstants.App);

        var appGuid = xApp?.Attribute(XmlConstants.Guid)?.Value;

        if (appGuid == null)
            return wrapLog.ReturnFalse(LogError("Something is wrong in the xml structure, can't get an app-guid"));

        if (appGuid != XmlConstants.AppContentGuid)
        {
            // Build Guid (take existing, or create a new)
            if (string.IsNullOrEmpty(appGuid) || appGuid == new Guid().ToString())
                appGuid = Guid.NewGuid().ToString();

            // Adding app to EAV
            var eavDc = base.Services.DbDataForNewApp.Value.Init(zoneId, null);

            // ParentApp
            parentAppId = GetParentAppId(xmlSource, eavDc);

            var app = eavDc.App.AddApp(null, appGuid, parentAppId);

            eavDc.SqlDb.SaveChanges();

            appId = app.AppId;
        }
        else
            appId = AppId;

        if (appId <= 0)
            return wrapLog.ReturnFalse(LogError("App was not created. Please try again or make sure the package you are importing is correct."));

        Log.A("Purging all Zones");
        base.Services.AppCachePurger.PurgeZoneList();
        return wrapLog.Return(ImportXml(zoneId, appId, doc), "done");
    }

    private static int? GetParentAppId(XElement xmlSource, Repository.Efc.DbDataController eavDc)
    {
        var parentAppXElement = xmlSource?.Element(XmlConstants.Header)?.Element(XmlConstants.ParentApp);
        if (parentAppXElement != null)
        {
            var parentAppGuidOrName = parentAppXElement?.Attribute(XmlConstants.Guid)?.Value;
            if (!string.IsNullOrEmpty(parentAppGuidOrName) && parentAppGuidOrName != XmlConstants.ParentApp)
            {
                if (int.TryParse(parentAppXElement?.Attribute(XmlConstants.AppId)?.Value, out int parAppId))
                    return eavDc.GetParentAppId(parentAppGuidOrName, parAppId);
            }
        }
        return null;
    }

}