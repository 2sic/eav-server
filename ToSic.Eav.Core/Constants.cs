﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav
{
    public class Constants
    {
        /// <summary>
        /// Name of the Default App in all Zones
        /// </summary>
        public const string DefaultAppName = "Default";
        /// <summary>
        /// Default Entity AssignmentObjectTypeId
        /// </summary>
        public const int DefaultAssignmentObjectTypeId = 1;
        public const string CultureSystemKey = "Culture";
        /// <summary>
        /// DataTimeline Operation-Key for Entity-States (Entity-Versioning)
        /// </summary>
        public const string DataTimelineEntityStateOperation = "s";

        #region DB Field / Names Constants

        /// <summary>
        /// AttributeSet StaticName must match this Regex. Accept Alphanumeric, except the first char must be alphabetic or underscore.
        /// </summary>
        public static string AttributeStaticNameRegEx = "^[_a-zA-Z]{1}[_a-zA-Z0-9]*";

        /// <summary>
        /// If AttributeSet StaticName doesn't match, users see this message.
        /// </summary>
        public static string AttributeStaticNameRegExNotes = "Only alphanumerics and underscore is allowed, first char must be alphabetic or underscore.";

        #endregion

        #region DataSource Constants

        /// <summary>
        /// Default ZoneId. Used if none is specified on the Context.
        /// </summary>
        public readonly static int DefaultZoneId = 1;
        /// <summary>
        /// AppId where MetaData (Entities) are stored.
        /// </summary>
        public readonly static int MetaDataAppId = 1;
        /// <summary>
        /// AssignmentObjectTypeId for FieldProperties (Field MetaData)
        /// </summary>
        public readonly static int AssignmentObjectTypeIdFieldProperties = 2;

        /// <summary>
        /// AssignmentObjectTypeId for DataPipelines
        /// </summary>
        public readonly static int AssignmentObjectTypeEntity = 4;

        public static readonly int AssignmentObjectTypeCmsObject = 10; 


        /// <summary>
        /// StaticName of the DataPipeline AttributeSet
        /// </summary>
        public readonly static string DataPipelineStaticName = "DataPipeline";
        /// <summary>
        /// StaticName of the DataPipelinePart AttributeSet
        /// </summary>
        public readonly static string DataPipelinePartStaticName = "DataPipelinePart";

        /// <summary>
        /// Attribute Name on the Pipeline-Entity describing the Stream-Wiring
        /// </summary>
        public const string DataPipelineStreamWiringStaticName = "StreamWiring";

        /// <summary>
        /// Default In-/Out-Stream Name
        /// </summary>
        public const string DefaultStreamName = "Default";

        /// <summary>PublishedEntities Stream Name</summary>
        public const string PublishedStreamName = "Published";
        /// <summary>Draft-Entities Stream Name</summary>
        public const string DraftsStreamName = "Drafts";

        public const string TypeForInputTypeDefinition = "ContentType-InputType";
        #endregion


        #region Version Change Constants
        public const string V3To4DataSourceDllOld = ", ToSic.Eav";
        public const string V3To4DataSourceDllNew = ", ToSic.Eav.DataSources";
        #endregion

        #region Scopes

        public const string ScopeSystem = "System";

        #endregion
    }
}
