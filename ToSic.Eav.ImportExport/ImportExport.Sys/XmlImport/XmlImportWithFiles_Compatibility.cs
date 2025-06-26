using System.Xml.Linq;
using ToSic.Eav.ImportExport.Sys.Xml;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

partial class XmlImportWithFiles
{
    public bool IsCompatible(XDocument doc)
    {
        var l = Log.Fn<bool>("is compatible check");
        var rootNodeList = doc.Elements(XmlConstants.RootNode);
        var rootNode = doc.Element(XmlConstants.RootNode);
        // Return if no Root Node "SexyContent"
        if (!rootNodeList.Any() || rootNode == null)
        {
            LogError("The XML file you specified does not seem to be a 2sxc/EAV Export.");
            return l.ReturnFalse("XML seems invalid");
        }

        var isEnvOk = IsCompatibleSingleVersion(rootNode,
            XmlConstants.MinEnvVersion,
            Services.Environment.TenantVersion
        );
        if (!isEnvOk)
            return l.ReturnFalse("Environment version check failed");

        var isSxcOk = IsCompatibleSingleVersion(rootNode,
            XmlConstants.MinModVersion,
            new(Services.Environment.ModuleVersion)
        );
        if (!isSxcOk)
            return l.ReturnFalse("2sxc version check failed");
        return l.ReturnTrue("all ok");
    }

    private bool IsCompatibleSingleVersion(XElement rootNode, string versionNodeName, Version currentVersion)
    {
        var l = Log.Fn<bool>();
        // Return if Version does not match
        var hasNoMinEnvVersionInXml = rootNode
            .Attributes()
            .All(a => a.Name != versionNodeName);

        if (hasNoMinEnvVersionInXml)
            return l.ReturnTrue("No min version specified");

        var minVersionString = rootNode.Attribute(versionNodeName)!.Value;
        var minVersionInXml = new Version(minVersionString);

        if (minVersionInXml <= currentVersion)
            return l.ReturnTrue("is compatible completed");

        LogError($"This template / app requires version {minVersionString}. You have version {currentVersion.ToString()} installed.");
        return l.ReturnFalse("XML version check failed");
    }
}