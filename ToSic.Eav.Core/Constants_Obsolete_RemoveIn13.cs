using System;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav
{
    public partial class Constants
    {
        #region Metadata Targets (Obsolete/Moved)

        /// <summary>Things that are not used as Metadata</summary>
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.None")]
        public static int NotMetadata = (int)TargetTypes.None; // 1;

        /// <summary>Metadata of attributes (fields)</summary>
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.Attribute")]
        public static readonly int MetadataForAttribute = (int)TargetTypes.Attribute; // 2;

        /// <summary>App metadata</summary>
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.App")]
        public static readonly int MetadataForApp = (int)TargetTypes.App;//  3;

        /// <summary>Metadata of entities (data-items)</summary>
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.Entity")]
        public const int MetadataForEntity = (int)TargetTypes.Entity; // 4;

        /// <summary>Metadata of a content-type (data-schema)</summary>
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.ContentType")]
        public static readonly int MetadataForContentType = (int)TargetTypes.ContentType; // 5;

        /// <summary>Zone metadata</summary>
        // ReSharper disable once UnusedMember.Global
        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.Zone")]
        public static readonly int MetadataForZone = (int)TargetTypes.Zone; // 6;

        [Obsolete("Will be removed in 2sxc 13 - use TargetTypes.CmsItem")]
        public static readonly int MetadataForCmsObject = (int)TargetTypes.CmsItem; // 10;
        #endregion


        #region Content-Types Constants (Obsolete/Moved)

        [Obsolete("Will be removed in 2sxc 13 - use ContentTypes")] public const string ContentTypeTypeName = ContentTypes.ContentTypeTypeName;
        [Obsolete("Will be removed in 2sxc 13 - use ContentTypes")] public static readonly string ContentTypeMetadataLabel = ContentTypes.ContentTypeMetadataLabel;

        #endregion

        #region Special Attributes / Fields of Entities in lower-case (Obsolete / Moved)

        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldTitle = Attributes.EntityFieldTitle;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldId = Attributes.EntityFieldId;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldAutoSelect = Attributes.EntityFieldAutoSelect;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldGuid = Attributes.EntityFieldGuid;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldType = Attributes.EntityFieldType;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldIsPublished = Attributes.EntityFieldIsPublished;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldCreated = Attributes.EntityFieldCreated;
        [Obsolete("Will be removed in 2sxc 13 - use Attributes.")] public const string EntityFieldModified = Attributes.EntityFieldModified;

        #endregion


        #region Special Field Types (Obsolete / Moved)

        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeBoolean = DataTypes.Boolean;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeNumber = DataTypes.Number;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeDateTime = DataTypes.DateTime;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeEntity = DataTypes.Entity;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeHyperlink = DataTypes.Hyperlink;
        [Obsolete("Will be removed in 2sxc 13 - use the new DataTypes")] public const string DataTypeString = DataTypes.String;

        #endregion

        #region Parameter protection (Obsolete / Moved)

        // Special constant to protect functions which should use named parameters
        [Obsolete("Will be removed in 2sxc 13 - use the new Parameters")]
        public const string RandomProtectionParameter = Parameters.Protector;

        // ReSharper disable once UnusedParameter.Local

        [Obsolete("Will be removed in 2sxc 13 - use the new Parameters")]
        public static void ProtectAgainstMissingParameterNames(string criticalParameter, string protectedMethod, string paramNames)
            => Parameters.ProtectAgainstMissingParameterNames(criticalParameter, protectedMethod, paramNames);

        #endregion

    }
}
