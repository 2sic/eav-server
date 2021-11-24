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
        /// Things that are not used as Metadata. This is the default for most Entities.
        /// </summary>
        None = 1,

        /// <summary>
        /// Metadata of attributes (fields). This is used to store configuration like the field label, amount-of-rows, etc.
        /// </summary>
        /// <remarks>
        /// The key is always a number (int) pointing to the Attribute ID in the DB.
        /// </remarks>
        Attribute = 2,

        /// <summary>
        /// App metadata. Used to give Apps additional properties. 
        /// </summary>
        /// <remarks>
        /// The key should always be an int ID of the App.
        /// </remarks>
        App = 3,

        /// <summary>
        /// Metadata of entities (data-items). 
        /// This lets us enhance entities with additional information. 
        /// </summary>
        /// <remarks>
        /// The Key should always be a GUID
        /// </remarks>
        Entity = 4,

        /// <summary>
        /// Metadata of a content-type (data-schema). Used to give it a description etc. 
        /// </summary>
        ContentType = 5,

        /// <summary>
        /// Zone metadata - used to give a Zone additonal information. 
        /// Only used in very special cases, best not use.
        /// </summary>
        // Used externally, for example in azing
        Zone = 6,

        /// <summary>
        /// Item / Object of the Platform, like a User, File etc.
        /// </summary>
        /// <remarks>
        /// * The key is usually a string to further specify what it's describing, like "file:72"
        /// * The text equivalent is CmsObject
        /// </remarks>
        CmsItem = 10,
    }
}
