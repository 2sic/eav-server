using System.Xml.Linq;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Repositories.Sys;

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
    public bool ImportApp(int zoneId, XDocument doc, int? inheritAppId, out int appId)
    {
        var l = Log.Fn<bool>($"zone:{zoneId}");

        appId = 0;
        int? parentAppId = null;

        if (!IsCompatible(doc))
            return l.ReturnFalse(LogError("The import file is not compatible with the installed version of 2sxc."));

        // Get root node "SexyContent"
        var xmlSource = doc.Element(XmlConstants.RootNode);
        var xApp = xmlSource?.Element(XmlConstants.Header)?.Element(XmlConstants.App);

        var appGuid = xApp?.Attribute(XmlConstants.Guid)?.Value;

        if (appGuid == null)
            return l.ReturnFalse(LogError("Something is wrong in the xml structure, can't get an app-guid"));

        // If it's not the default app, then we need to create it
        if (appGuid != XmlConstants.AppContentGuid)
        {
            // Build Guid (take existing, or create a new)
            if (string.IsNullOrEmpty(appGuid) || appGuid == new Guid().ToString())
                appGuid = Guid.NewGuid().ToString();

            // Adding app to EAV
            //var eavDc = Services.DbDataForNewApp.Value.Init(zoneId, null);
            var storage = Services.StorageFactory.New(new(zoneId, null, null));

            // ParentApp
            parentAppId = inheritAppId ?? GetParentAppId(xmlSource, storage);

            // #WipDecoupleDbFromImport
            //var app = eavDc.App.AddApp(null, appGuid, parentAppId);
            //eavDc.SqlDb.SaveChanges();
            //appId = app.AppId;

            appId = storage.CreateApp(appGuid, parentAppId);
        }
        // Otherwise use the current app (the Content/Default app) to import into
        else
            appId = AppId;

        if (appId <= 0)
            return l.ReturnFalse(LogError("App was not created. Please try again or make sure the package you are importing is correct."));

        l.A("Purging all Zones");
        Services.AppCachePurger.PurgeZoneList();
        var result = ImportXml(zoneId, appId, parentAppId, doc);
        return l.Return(result, "done");
    }

    private int? GetParentAppId(XElement xmlSource, IStorage eavDc)
    {
        var l = Log.Fn<int?>();
        var parentAppXElement = xmlSource?.Element(XmlConstants.Header)?.Element(XmlConstants.ParentApp);
        if (parentAppXElement == null)
            return l.ReturnNull("app doesn't have inherit-data");
        
        var parentAppGuidOrName = parentAppXElement.Attribute(XmlConstants.Guid)?.Value;
        if (string.IsNullOrEmpty(parentAppGuidOrName) || parentAppGuidOrName == XmlConstants.ParentApp)
            return l.ReturnNull("app inherits default or nothing");

        var parentIdString = parentAppXElement.Attribute(XmlConstants.AppId)?.Value;
        if (!int.TryParse(parentIdString, out var parAppId))
            return l.ReturnNull($"app inherit data not useful int: {parentIdString}");
        
        var parentAppId = eavDc.GetParentAppId(parentAppGuidOrName, parAppId);
        return l.Return(parentAppId, $"parentAppId: {parentAppId}");
    }

}