using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// Metadata targets specific things, and the TargetTypes determines what kind of thing this is. 
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public enum TargetTypes
    {
        /// <summary>
        /// Undefined Type (0) - included for completeness
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Things that are not used as Metadata (1). This is the default for most Entities.
        /// </summary>
        None = 1,

        /// <summary>
        /// Metadata of attributes / fields (2). This is used to store configuration like the field label, amount-of-rows, etc.
        /// </summary>
        /// <remarks>
        /// The key is always a number (int) pointing to the Attribute ID in the DB.
        /// </remarks>
        Attribute = 2,

        /// <summary>
        /// App metadata (3). Used to give Apps additional properties. 
        /// </summary>
        /// <remarks>
        /// The key should always be an int ID of the App.
        /// </remarks>
        App = 3,

        /// <summary>
        /// Metadata of entities / data-items (4). 
        /// This lets us enhance entities with additional information. 
        /// </summary>
        /// <remarks>
        /// The Key should always be a GUID
        /// </remarks>
        Entity = 4,

        /// <summary>
        /// Metadata of a content-type / data-schema (5). Used to give it a description etc. 
        /// </summary>
        ContentType = 5,

        /// <summary>
        /// Zone metadata (6) - used to give a Zone additional information. 
        /// Only used in very special cases, best not use.
        /// </summary>
        // Used externally, for example in azing
        Zone = 6,

        /// <summary>
        /// Item / Object of the Platform, like a File or Folder etc. (10)
        /// </summary>
        /// <remarks>
        /// * The key is usually a string to further specify what it's describing, like "file:72"
        /// * The text equivalent is CmsObject
        /// </remarks>
        CmsItem = 10,

        /// <summary>
        /// The entire system / platform - so Metadata for the current Dnn/Oqtane installation (11).
        /// </summary>
        /// <remarks>
        /// This is not in use as of now, just added for completeness sakes.
        /// New in v13
        /// </remarks>
        System = 11,

        /// <summary>A Site - like the current site (12)</summary>
        /// <remarks>New in v13</remarks>
        Site = 12,

        /// <summary>A Site - like the current site (13)</summary>
        /// <remarks>New in v13 / beta</remarks>
        [PrivateApi]
        SiteVariant = 13,

        /// <summary>A Page - like the current page (14)</summary>
        /// <remarks>New in v13</remarks>
        Page = 14,

        /// <summary>A Page - like the current page (15)</summary>
        /// <remarks>New in v13 / beta</remarks>
        [PrivateApi]
        PageVariant = 15,

        /// <summary>A Module - like a module containing some content (16)</summary>
        /// <remarks>New in v13</remarks>
        Module = 16,

        /// <summary>A Module - like a module containing some content (17)</summary>
        /// <remarks>New in v13 / beta</remarks>
        [PrivateApi]
        ModuleVariant = 17,

        /// <summary>A User - like the admin-user (18)</summary>
        /// <remarks>New in v13</remarks>
        User = 18,

        /// <summary>
        /// Custom target (90). This is what you should use for basic apps which have a custom target that's none of the other defaults.
        /// </summary>
        Custom = 90,

        /// <summary>Custom target (91). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom1 = 91,
        /// <summary>Custom target (92). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom2 = 92,
        /// <summary>Custom target (93). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom3 = 93,
        /// <summary>Custom target (94). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom4 = 94,
        /// <summary>Custom target (95). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom5 = 95,
        /// <summary>Custom target (96). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom6 = 96,
        /// <summary>Custom target (97). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom7 = 97,
        /// <summary>Custom target (98). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom8 = 98,
        /// <summary>Custom target (99). Use this for basic apps which need multiple different custom targets (advanced, rare use case)</summary>
        Custom9 = 99,

    }
}
