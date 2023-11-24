using System;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Internal.Environment;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.ImportExport.ImportHelpers;

internal class VersionCheck: HelperBase
{
    private readonly IImportExportEnvironment _environment;

    public VersionCheck(IImportExportEnvironment env, ILog parentLog) : base(parentLog, "Imp.VerChk")
    {
        _environment = env;
    }

    internal void EnsureVersions(XElement appConfig) => Log.Do(() =>
    {
        var reqVersionNode = appConfig.Elements(XmlConstants.ValueNode)
            .FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "RequiredVersion")
            ?.Attribute(XmlConstants.ValueAttr)?.Value;
        var reqVersionNodeDnn = appConfig.Elements(XmlConstants.ValueNode)
            .FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "RequiredDnnVersion")
            ?.Attribute(XmlConstants.ValueAttr)?.Value;

        CheckRequiredEnvironmentVersions(reqVersionNode, reqVersionNodeDnn);
    });

    private void CheckRequiredEnvironmentVersions(string reqVersionNode, string reqVersionNodePlatform
    ) => Log.Do($"{reqVersionNode}, {reqVersionNodePlatform}", () =>
    {
        if (reqVersionNode != null)
        {
            var vEav = Version.Parse(_environment.ModuleVersion);
            var reqEav = Version.Parse(reqVersionNode);
            if (reqEav.CompareTo(vEav) == 1) // required is bigger
                throw new Exception("this app requires eav/2sxc version " + reqVersionNode +
                                    ", installed is " + vEav +
                                    ". cannot continue. see also 2sxc.org/en/help?tag=app");
        }

        if (reqVersionNodePlatform != null)
        {
            var vHost = _environment.TenantVersion;
            var reqHost = Version.Parse(reqVersionNodePlatform);
            if (reqHost.CompareTo(vHost) == 1) // required is bigger
                throw new Exception("this app requires host/dnn version " + reqVersionNodePlatform +
                                    ", installed is " + vHost +
                                    ". cannot continue. see also 2sxc.org/en/help?tag=app");
        }
    });

}