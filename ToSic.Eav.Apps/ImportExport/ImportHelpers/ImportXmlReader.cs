﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.ImportExport.ImportHelpers;

/// <summary>
/// Read an xml file, check for headers and verify all the parts to better process the import
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImportXmlReader: HelperBase
{
    public ImportXmlReader(string xmlPath, XmlImportWithFiles importer, ILog parentLog) : base(parentLog, "Imp.XmlPrt")
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
    internal XElement AppConfig => _appConfig ??= GetAppConfig();

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
    public string AppFolder => _appFolder ??= GetKeyValueOrThrowOnNull("Folder");

    #endregion

    public string DisplayName => GetKeyValueOrThrowOnNull(nameof(DisplayName));
    public string Description => GetKeyValue(nameof(Description)) ?? string.Empty;
    public string Version => GetKeyValueOrThrowOnNull(nameof(Version));


    private string GetKeyValueOrThrowOnNull(string key)
    {
        var value = GetKeyValue(key);

        if (value == null)
            throw new NullReferenceException($"can't determine {key} from xml, cannot continue");
        return value;
    }

    private string GetKeyValue(string key)
    {
        ThrowErrorIfNotAppImport();
 
        return AppConfig.Elements(XmlConstants.ValueNode)
            .FirstOrDefault(v => string.Equals(v.Attribute(XmlConstants.KeyAttr)?.Value, key, StringComparison.InvariantCultureIgnoreCase))
            ?.Attribute(XmlConstants.ValueAttr)
            ?.Value;
    }

    private void ThrowErrorIfNotAppImport()
    {
        if (!IsAppImport) throw new Exception("not app import, this shouldn't be accessed!");
    }
}