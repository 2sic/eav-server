namespace ToSic.Eav.Metadata
{

    public enum TargetTypes
    {
        /// <summary>Things that are not used as Metadata</summary>
        None = 1,

        /// <summary>Metadata of attributes (fields)</summary>
        Attribute = 2,

        /// <summary>App metadata</summary>
        App = 3,

        /// <summary>Metadata of entities (data-items)</summary>
        Entity = 4,

        /// <summary>Metadata of a content-type (data-schema)</summary>
        ContentType = 5,

        /// <summary>Zone metadata</summary>
        // Used externally, for example in azing
        Zone = 6,

        /// <summary>
        /// Item / Object of the Platform, like a User, File etc.
        /// </summary>
        /// <remarks>The text equivalent is CmsObject</remarks>
        CmsItem = 10,

}
}
