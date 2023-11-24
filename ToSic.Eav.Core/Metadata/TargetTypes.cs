using ToSic.Lib.Documentation;

namespace ToSic.Eav.Metadata;

/// <summary>
/// Metadata targets specific things, and the TargetTypes determines what kind of thing this is. 
/// </summary>
[PublicApi]
public enum TargetTypes
{
    /// <summary>
    /// Undefined Type (0) - included for completeness
    /// </summary>
    [PrivateApi]
    [DocsWip("(undefined) - included for completeness, should not be used.")]
    Undefined = 0,

    /// <summary>
    /// Things that are not used as Metadata (1). This is the default for most Entities.
    /// </summary>
    [DocsWip("(none) / not Metadata - Things that are not used as Metadata (1). This is the default for most Entities.")]
    None = 1,

    /// <summary>
    /// Metadata of attributes / fields (2). This is used to store configuration like the field label, amount-of-rows, etc.
    /// </summary>
    /// <remarks>
    /// The key is always a number (int) pointing to the Attribute ID in the DB.
    /// </remarks>
    [DocsWip("Attributes / Fields (2). This is used to store configuration like the field label, amount-of-rows, etc.")]
    Attribute = 2,

    /// <summary>
    /// App metadata (3). Used to give Apps additional properties. 
    /// </summary>
    /// <remarks>
    /// The key should always be an int ID of the App.
    /// </remarks>
    [DocsWip("Apps (3). Used to give Apps additional properties.")]
    App = 3,

    /// <summary>
    /// Metadata of entities / data-items (4). 
    /// This lets us enhance entities with additional information. 
    /// </summary>
    /// <remarks>
    /// The Key should always be a GUID
    /// </remarks>
    [DocsWip("Entities (4) - decorates Entities")]
    Entity = 4,

    /// <summary>
    /// Metadata of a content-type / data-schema (5). Used to give it a description etc. 
    /// </summary>
    [DocsWip("Content-Types (5)")]
    ContentType = 5,

    /// <summary>
    /// Zone metadata (6) - used to give a Zone additional information. 
    /// Only used in very special cases, best not use.
    /// </summary>
    // Used externally, for example in azing
    [DocsWip("Zones (6)")]
    Zone = 6,

    /// <summary>
    /// Scope metadata (7) - for data-scopes like "System" or "System-Configuration" etc.
    /// </summary>
    [DocsWip("Scopes (7)")]
    Scope = 7,

    /// <summary>
    /// Dimension Metadata (8) - for languages and similar data-dimensions
    /// </summary>
    [DocsWip("Dimensions (8)")]
    Dimension = 8,


    /// <summary>
    /// Item / Object of the Platform, like a File or Folder etc. (10)
    /// </summary>
    /// <remarks>
    /// * The key is usually a string to further specify what it's describing, like "file:72"
    /// * The text equivalent is CmsObject
    /// </remarks>
    [DocsWip("CMS Items (10) such as files, folders, etc.")]
    CmsItem = 10,

    /// <summary>
    /// The entire system / platform - so Metadata for the current Dnn/Oqtane installation (11).
    /// </summary>
    /// <remarks>
    /// This is not in use as of now, just added for completeness sakes.
    /// New in v13
    /// </remarks>
    [DocsWip("System (11)")]
    System = 11,

    /// <summary>A Site - like the current site (12)</summary>
    /// <remarks>New in v13</remarks>
    [DocsWip("Sites (12)")]
    Site = 12,

    /// <summary>A Site - like the current site (13)</summary>
    /// <remarks>New in v13 / beta</remarks>
    [PrivateApi]
    SiteVariant = 13,

    /// <summary>A Page - like the current page (14)</summary>
    /// <remarks>New in v13</remarks>
    [DocsWip("Pages (13)")]
    Page = 14,

    /// <summary>A Page - like the current page (15)</summary>
    /// <remarks>New in v13 / beta</remarks>
    [PrivateApi]
    PageVariant = 15,

    /// <summary>A Module - like a module containing some content (16)</summary>
    /// <remarks>New in v13</remarks>
    [DocsWip("Modules (16)")]
    Module = 16,

    /// <summary>A Module - like a module containing some content (17)</summary>
    /// <remarks>New in v13 / beta</remarks>
    [PrivateApi]
    ModuleVariant = 17,

    /// <summary>A User - like the admin-user (18)</summary>
    /// <remarks>New in v13</remarks>
    [DocsWip("Users (18)")]
    User = 18,


    [PrivateApi("can be used for something later on")]
    Reserved19 = 19,

    #region 20-39 - probably more common objects



    #endregion

    #region MyRegion

        

    #endregion

    // TODO:
    [PrivateApi("not ready yet, still in concept, may change")]
    License = 20,

    [PrivateApi("not ready yet, still in concept, may change")]
    Feature = 21,

    [PrivateApi("can be used for something later on")]
    MetadataTargetType = 22,

    [PrivateApi("can be used for something later on")]
    Reserved23 = 23,

    [PrivateApi("not ready yet, still in concept, may change")]
    EntityValue = 24,


    /// <summary>
    /// Custom target (90). This is what you should use for basic apps which have a custom target that's none of the other defaults.
    /// </summary>
    [DocsWip("Custom #0 (90) - Use for whatever you want.")]
    Custom = 90,

    /// <summary>Custom target (91). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #1 (91) - Use for whatever you want.")]
    Custom1 = 91,
    /// <summary>Custom target (92). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #2 (92) - Use for whatever you want.")]
    Custom2 = 92,
    /// <summary>Custom target (93). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #3 (93) - Use for whatever you want.")]
    Custom3 = 93,
    /// <summary>Custom target (94). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #4 (94) - Use for whatever you want.")]
    Custom4 = 94,
    /// <summary>Custom target (95). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #5 (95) - Use for whatever you want.")]
    Custom5 = 95,
    /// <summary>Custom target (96). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #6 (96) - Use for whatever you want.")]
    Custom6 = 96,
    /// <summary>Custom target (97). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #7 (97) - Use for whatever you want.")]
    Custom7 = 97,
    /// <summary>Custom target (98). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #8 (98) - Use for whatever you want.")]
    Custom8 = 98,
    /// <summary>Custom target (99). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
    [DocsWip("Custom #9 (99) - Use for whatever you want.")]
    Custom9 = 99,

}