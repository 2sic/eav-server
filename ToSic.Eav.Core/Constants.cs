using System;
using System.Text.RegularExpressions;

namespace ToSic.Eav
{
    public partial class Constants
    {
        /// <summary>
        /// Name of the Default App in all Zones
        /// </summary>
        public const string DefaultAppName = "Default";

        public const int AppIdEmpty = 0;

        public const int NullId = -2742;
        public const int IdNotInitialized = -999;
        public const string UrlNotInitialized = "url-not-initialized";

        public const string CultureSystemKey = "Culture";

        /// <summary>
        /// DataTimeline Operation-Key for Entity-States (Entity-Versioning)
        /// </summary>
        public const string DataTimelineEntityJson = "e";

        #region DB Field / Names Constants

        /// <summary>
        /// AttributeSet StaticName must match this Regex. Accept Alphanumeric, except the first char must be alphabetic or underscore.
        /// </summary>
        public static Regex AttributeStaticName =
            new Regex("^[_a-zA-Z]{1}[_a-zA-Z0-9]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// If AttributeSet StaticName doesn't match, users see this message.
        /// </summary>
        public static string AttributeStaticNameRegExNotes =
            "Only alphanumerics and underscore is allowed, first char must be alphabetic or underscore.";

        #endregion

        #region DataSource Constants

        /// <summary>
        /// Default ZoneId. Used if none is specified on the Context.
        /// </summary>
        public static readonly int DefaultZoneId = 1;

        /// <summary>
        /// AppId where MetaData (Entities) are stored.
        /// </summary>
        public static readonly int MetaDataAppId = 1;

        /// <summary>Things that are not used as Metadata</summary>
        public const int NotMetadata = 1;

        /// <summary>Metadata of attributes (fields)</summary>
        public static readonly int MetadataForAttribute = 2;

        /// <summary>App metadata</summary>
        public static readonly int MetadataForApp = 3;

        /// <summary>Metadata of entities (data-items)</summary>
        public const int MetadataForEntity = 4;

        /// <summary>Metadata of a content-type (data-schema)</summary>
        public static readonly int MetadataForContentType = 5;

        public const string ContentTypeTypeName = "ContentType";


        /// <summary>Zone metadata</summary>
        /// <remarks>Used externally, for example in azing</remarks>
        // ReSharper disable once UnusedMember.Global
        public static readonly int MetadataForZone = 6;

        public static readonly int MetadataForCmsObject = 10;

        #region Metadata-Properties which have system use

        public static readonly string ContentTypeMetadataLabel = "Label";

        #endregion

        /// <summary>content type name of the query AttributeSet</summary>
        public static readonly string QueryTypeName = "DataPipeline";

        /// <summary>content-type name of the queryPart AttributeSet</summary>
        public static readonly string QueryPartTypeName = "DataPipelinePart";

        /// <summary>
        /// Attribute Name on the query-Entity describing the Stream-Wiring
        /// </summary>
        public const string QueryStreamWiringAttributeName = "StreamWiring";

        /// <summary>
        /// Default In-/Out-Stream Name
        /// </summary>
        public const string DefaultStreamName = "Default";
        public const string DefaultStreamNameRequired = DefaultStreamName + "*";

        public const string FallbackStreamName = "Fallback";

        /// <summary>PublishedEntities Stream Name</summary>
        public const string PublishedStreamName = "Published";

        /// <summary>Draft-Entities Stream Name</summary>
        public const string DraftsStreamName = "Drafts";

        public const string RepoIdInternalField = "_RepositoryId";
        public const string IsPublishedField = "IsPublished";
        public const string PublishedEntityField = "PublishedEntity";
        public const string DraftEntityField = "DraftEntity";


        public const string MetadataFieldTypeAll = "@All";
        public const string MetadataFieldAllInputType = "InputType";
        public const string MetadataFieldTypeString = "@String";

        public const string TypeForInputTypeDefinition = "ContentType-InputType";

        public const string InputTypeType = "Type";
        public const string InputTypeLabel = "Label";
        public const string InputTypeDescription = "Description";
        public const string InputTypeAssets = "Assets";
        public const string InputTypeDisableI18N = "DisableI18n";
        public const string InputTypeAngularAssets = "AngularAssets";
        public const string InputTypeUseAdam = "UseAdam";


        public const string MetadataFieldAllIsEphemeral = "IsEphemeral";
        public const string MetadataFieldAllFormulas = "Formulas";

        #endregion


        #region Version Change Constants

        public const string V3To4DataSourceDllOld = ", ToSic.Eav";
        public const string V3To4DataSourceDllNew = ", ToSic.Eav.DataSources";

        #endregion

        #region Scopes

        public const string ScopeSystem = "System";

        #endregion

        #region System field names - how they are shown / used publicly

        /// <summary>
        /// This is for public values / fields - like when the title is streamed in an API
        /// </summary>
        public const string SysFieldTitle = "Title";
        public const string SysFieldModified = "Modified";
        public const string SysFieldCreated = "Created";
        public const string SysFieldGuid = "Guid";
        
        #endregion
        

        #region Special Properties of Entities

        public const string EntityFieldTitle = "entitytitle";
        public const string EntityFieldId = "entityid";
        public const string EntityFieldAutoSelect = "entity-title-id";
        public const string EntityFieldGuid = "entityguid";
        public const string EntityFieldType = "entitytype";
        public const string EntityFieldIsPublished = "ispublished";
        public const string EntityFieldModified = "modified";
        public const string EntityFieldCreated= "created";

        /// <summary>
        /// Virtual fields are not real fields, but information properties like title, etc.
        /// </summary>
        public const string EntityFieldIsVirtual = "virtual";

        public static bool InternalOnlyIsSpecialEntityProperty(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case EntityFieldTitle:
                case EntityFieldId:
                case EntityFieldGuid:
                case EntityFieldType:
                case EntityFieldIsPublished:
                case EntityFieldCreated:
                case EntityFieldModified:
                    return true;
            }
            return false;

        }

        #endregion

        #region Special Field Types

        public const string DataTypeBoolean = "Boolean";
        public const string DataTypeNumber = "Number";
        public const string DataTypeDateTime = "DateTime";
        public const string DataTypeEntity = "Entity"; // todo: update all references with this as a constant
        public const string DataTypeHyperlink = "Hyperlink";
        public const string DataTypeString = "String";
        #endregion


        #region special uses of Apps
        public const string ContentAppName = "Content";
        public const string ContentAppFolder = "Content";
        public const string AppAssignmentName = "App";


        #endregion

        public const string DynamicType = "dynamic";

        public const int TransientAppId = -9999999;
        public const int SystemContentTypeFakeParent = -9203503; // just a very strange, dummy number

        #region Parameter protection
        // Special constant to protect functions which should use named parameters
        public const string RandomProtectionParameter = "all params must be named, like 'enable: true, language: ''de-ch'' - see https://docs.2sxc.org/net-code/conventions/named-parameters.html";
        // ReSharper disable once UnusedParameter.Local
        public static void ProtectAgainstMissingParameterNames(string criticalParameter, string protectedMethod, string paramNames)
        {
            if (criticalParameter == null || criticalParameter != RandomProtectionParameter)
                throw new Exception($"when using '{protectedMethod}' you must use named parameters " +
                                    "- otherwise you are relying on the parameter order staying the same. " +
                                    $"this command experts params like {paramNames}");
        }


        #endregion
    }
}
