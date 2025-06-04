using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.ImportExport.Internal.Xml;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class XmlConstants
{
    // nodes which are actually 2sxc-specific, and should be refactored some time
    public const string Root = "SexyContentData";
    public const string Root97 = "ContentData"; // import supports new name in 2sxc 9.7, but we won't export it with this name yet

    public const string EntityGuid = AttributeNames.GuidNiceName;

    public const string EntityLanguage = "Language";

    public const string EntityTypeAttribute = "Type";


    public const string Type = "Type";
    public const string IsTitle = "IsTitle";
    public const string Static = "StaticName";
    public const string Name = "Name";
    public const string Description = "Description";
    public const string Scope = "Scope";
    public const string AlwaysShareConfig = "AlwaysShareConfiguration";
    public const string SortAttributes = "SortAttributes";


    public const string RootNode = "SexyContent";
    public const string RootNode97 = "EavContent";
    public const string Header = "Header";
    public const string FileVersion = "FileVersion";
    public const string ExportDate = "ExportDate";
    public const string Culture = "Culture";
    public const string CultureSysKey = "SystemKey";
    public const string CultureExtKey = "ExternalKey";
    public const string CultureIsActiveAttrib = "Active";
    public const string App = "App";
    public const string AppId = "AppId";
    public const string ParentApp = "ParentApp";
    public const string AppContentGuid = "Default";
    public const string Guid = AttributeNames.GuidNiceName;
    public const string GuidNode = "EntityGUID";

    // attributes / sets
    public const string AttributeSets = "AttributeSets";
    public const string AttributeSet = "AttributeSet";
    public const string AttSetStatic = "AttributeSetStaticName";
    public const string AttSetNiceName = "AttributeSetName";
    public const string Entities = "Entities";
    public const string Entity = "Entity";
    public const string ContentType = "ContentType";
    public const string CmsObject = "CmsObject";
    public const string Attributes = "Attributes";
    public const string Attribute = "Attribute";
    public const string AttributeSetParentDef = "UsesConfigurationOfAttributeSet";

    public const string IsPublished = "IsPublished";
    public const string SysSettings = "SysSettings"; // #SharedFieldDefinition

    // Keys for metadata
    public const string KeyTargetTypeNameOld = "AssignmentObjectType";
    public const string KeyTargetType = "TargetType"; // #TargetTypeIdInsteadOfTarget
    public const string KeyGuid = "KeyGuid";
    public const string KeyNumber = "KeyNumber";
    public const string KeyString = "KeyString";

    // files
    public const string ValueTypeLink = "Hyperlink";
    public const string PortalFiles = "PortalFiles";
    public const string FileIdInRegEx = "FileId";
    public const string FileRefRegex = "^File:(?<FileId>[0-9]+)";
    public const string FileNode = "File";
    public const string FileIdAttr = "Id";

    // Folder stuff
    public const string FolderGroup = "PortalFolders";
    public const string Folder = "Folder";
    public const string FolderNodeId = "Id";
    public const string FolderNodePath = "RelativePath";

    // Values
    public const string ValueNode = "Value";
    public const string KeyAttr = "Key";
    public const string ValueAttr = "Value";
    public const string ValueDimNode = "Dimension";
    public const string ValueDimRoAttr = "ReadOnly";

    // JSON Entities
    public const string EntityIsJsonAttribute = "Json";

    // Dimensions
    public const string DimensionDefinition = "Dimensions";
    public const string DimensionDefElement = "Dimension";
    public const string DimId = "DimensionID";
    public const string Language = "Language";
    public const string LangDefault = "Default";

    // Templates
    // note: many nodes are also in AppConstants, as they are actually standard nodes in the template-entity
    // 2025-03-11 2dm disabled, as we removed the old obsolete XML import for these nodes
    // https://github.com/2sic/2sxc/issues/3598
    //public const string Templates = "Templates";
    //public const string Template = "Template";
    //public const string TemplateItemType = "ItemType";
    //public const string TemplateContentTypeId = "ContentTypeID";
    //public const string TemplateDemoItemId = "DemoEntityID";
    //public const string TemplateDemoItemGuid = "DemoEntityGUID";
    //public const string TemplateQueryGuidField = "PipelineEntityGUID";


    // Virtual Table Export - Placeholders/codes
    public const string NullMarker = "[]";
    public const string EmptyMarker = "[\"\"]";
    public const string ReadOnly = "ro";
    public const string ReadWrite = "rw";

    // Versioning information
    public const string MinEnvVersion = "MinimumRequiredVersion";
    public const string MinModVersion = "ModuleVersion";
}