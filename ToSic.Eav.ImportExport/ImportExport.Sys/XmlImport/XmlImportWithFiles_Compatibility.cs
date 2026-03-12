using System.Xml.Linq;
using ToSic.Eav.ImportExport.Sys.Xml;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Sys.XmlImport;

partial class XmlImportWithFiles
{
    public string? IsCompatible(XDocument doc, bool skipSxcVersionCheck)
    {
        var l = Log.Fn<string>();
        var rootNodeList = doc.Elements(XmlConstants.RootNode);
        var rootNode = doc.Element(XmlConstants.RootNode);
        // Return if no Root Node "SexyContent"
        if (!rootNodeList.Any() || rootNode == null)
            return l.ReturnAndLog(LogError("The XML file you specified does not seem to be a 2sxc/EAV Export."));

        var isEnvOk = IsCompatibleSingleVersion(rootNode,
            XmlConstants.MinEnvVersion,
            Services.Environment.TenantVersion,
            "Environment Version"
        );
        if (!isEnvOk)
            return l.ReturnAndLog("Environment version check failed");

        // 2026-03-12 2dm - introduced this, because all the exported files have the version of the exporting 2sxc
        // but often that doesn't actually matter.
        // Before this would prevent the import
        // Now it will just log that the check was skipped, and continue with the import.
        if (skipSxcVersionCheck)
            return l.ReturnNull("skipped 2sxc version check");

        var isSxcOk = IsCompatibleSingleVersion(rootNode,
            XmlConstants.MinModVersion,
            new(Services.Environment.ModuleVersion),
            "2sxc Version"
        );

        return !isSxcOk
            ? l.ReturnAndLog("2sxc version check failed")
            : l.ReturnNull("all ok");
    }

    private bool IsCompatibleSingleVersion(XElement rootNode, string versionNodeName, Version currentVersion, string versionInfo)
    {
        var l = Log.Fn<bool>($"version to check: {versionInfo}");
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

        var msg = $"This template / app requires v{minVersionString} for {versionInfo}. You have v{currentVersion}.";
        return l.ReturnFalse($"XML version check failed. {LogError(msg)}");
    }
}