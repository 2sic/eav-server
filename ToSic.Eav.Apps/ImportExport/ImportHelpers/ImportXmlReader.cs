using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Lib.Logging;


namespace ToSic.Eav.Apps.ImportExport.ImportHelpers
{
    /// <summary>
    /// Read an xml file, check for headers and verify all the parts to better process the import
    /// </summary>
    public class ImportXmlReader: HasLog
    {
        public ImportXmlReader(string xmlPath, XmlImportWithFiles importer, ILog parentLog) : base("Imp.XmlPrt", parentLog, nameof(ImportXmlReader))
        {
            XmlPath = xmlPath;

            FileContents = File.ReadAllText(xmlPath);
            XmlDoc = XDocument.Parse(FileContents);

            if (!importer.IsCompatible(XmlDoc))
                throw new Exception("The app / package is not compatible with this version of eav and the 2sxc-host.");


            Root = XmlDoc.Element(XmlConstants.RootNode)
                           ?? throw new NullReferenceException("xml root node couldn't be found");
            Header = Root.Element(XmlConstants.Header)
                           ?? throw new NullReferenceException("xml header node couldn't be found");

            IsAppImport = Header.Elements(XmlConstants.App).Any()
                              && Header.Element(XmlConstants.App)?.Attribute(XmlConstants.Guid)?.Value !=
                              XmlConstants.AppContentGuid;

        }

        internal readonly string XmlPath;

        internal string FileContents { get; }

        public XDocument XmlDoc;

        internal XElement Root;

        internal XElement Header;

        internal bool IsAppImport;

        #region AppConfig
        private XElement _appConfig;
        internal XElement AppConfig => _appConfig ?? (_appConfig = GetAppConfig());

        private XElement GetAppConfig()
        {
            ThrowErrorIfNotAppImport();

            var appConfig = Root
                .Element(XmlConstants.Entities)?
                .Elements(XmlConstants.Entity)
                .Single(e => e.Attribute(XmlConstants.AttSetStatic)?.Value == AppLoadConstants.TypeAppConfig);

            if (appConfig == null)
                throw new NullReferenceException("app config node not found in xml, cannot continue");
            
            return appConfig;
        }
        #endregion

        #region AppFolder

        private string _appFolder;
        public string AppFolder => _appFolder ?? (_appFolder = GetAppKeyValue("Folder"));

        #endregion

        public string DisplayName => GetAppKeyValue(nameof(DisplayName));
        public string Description => GetAppKeyValue(nameof(Description));
        public string Version => GetAppKeyValue(nameof(Version));


        private string GetAppKeyValue(string key)
        {
            ThrowErrorIfNotAppImport();

            var keyLower = key.ToLowerInvariant();
            var value = AppConfig.Elements(XmlConstants.ValueNode)
                .First(v => v.Attribute(XmlConstants.KeyAttr)?.Value.ToLowerInvariant() == keyLower)
                .Attribute(XmlConstants.ValueAttr)
                ?.Value;

            if (value == null)
                throw new NullReferenceException($"can't determine {key} from xml, cannot continue");
            return value;
        }

        private void ThrowErrorIfNotAppImport()
        {
            if (!IsAppImport) throw new Exception("not app import, this shouldn't be accessed!");
        }
    }
}
