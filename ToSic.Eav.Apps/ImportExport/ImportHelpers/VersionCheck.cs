using System;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Interfaces;


namespace ToSic.Eav.Apps.ImportExport.ImportHelpers
{
    internal class VersionCheck: HasLog
    {
        private readonly IImportExportEnvironment _environment;

        public VersionCheck(IImportExportEnvironment env, ILog parentLog) : base("Imp.VerChk", parentLog)
        {
            _environment = env;
        }

        internal void EnsureVersions(XElement appConfig)
        {
            var wrapLog = Log.Fn();
            var reqVersionNode = appConfig.Elements(XmlConstants.ValueNode)
                .FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "RequiredVersion")
                ?.Attribute(XmlConstants.ValueAttr)?.Value;
            var reqVersionNodeDnn = appConfig.Elements(XmlConstants.ValueNode)
                .FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "RequiredDnnVersion")
                ?.Attribute(XmlConstants.ValueAttr)?.Value;

            CheckRequiredEnvironmentVersions(reqVersionNode, reqVersionNodeDnn);
            wrapLog.Done("ok");
        }

        private void CheckRequiredEnvironmentVersions(string reqVersionNode, string reqVersionNodeDnn)
        {
            var wrapLog = Log.Fn($"{reqVersionNode}, {reqVersionNodeDnn}");
            if (reqVersionNode != null)
            {
                var vEav = Version.Parse(_environment.ModuleVersion);
                var reqEav = Version.Parse(reqVersionNode);
                if (reqEav.CompareTo(vEav) == 1) // required is bigger
                    throw new Exception("this app requires eav/2sxc version " + reqVersionNode +
                                        ", installed is " + vEav + ". cannot continue. see also 2sxc.org/en/help?tag=app");
            }

            if (reqVersionNodeDnn != null)
            {
                var vHost = _environment.TenantVersion;
                var reqHost = Version.Parse(reqVersionNodeDnn);
                if (reqHost.CompareTo(vHost) == 1) // required is bigger
                    throw new Exception("this app requires host/dnn version " + reqVersionNodeDnn +
                                        ", installed is " + vHost + ". cannot continue. see also 2sxc.org/en/help?tag=app");
            }
            wrapLog.Done("completed");
        }

    }
}
