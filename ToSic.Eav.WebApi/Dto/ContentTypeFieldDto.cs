using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Data;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi.Dto
{
    public class ContentTypeFieldDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; }
        public string InputType { get; set; }
        public string StaticName { get; set; }
        public bool IsTitle { get; set; }
        public int AttributeId { get; set; }
        public IDictionary<string, EavLightEntity> Metadata { get; set; }
        public InputTypeInfo InputTypeConfig { get; set; }

        public HasPermissionsDto Permissions { get; set; }
        
        /// <summary>
        /// Tells the system that it will not save the field value / temporary
        /// </summary>
        /// <remarks>
        /// New in v12.01
        /// </remarks>
        public bool IsEphemeral { get; set; }
        
        /// <summary>
        /// Information if the field has calculations attached
        /// </summary>
        /// <remarks>
        /// New in v12.01
        /// </remarks>
        public bool HasFormulas { get; set; }

        public EditInfoAttributeDto EditInfo { get; set; }

        // #SharedFieldDefinition
        public Guid? Guid { get; set; }

        public JsonAttributeSysSettings SysSettings { get; set; }

        /// <summary>
        /// Short info for the case where we get the fields of many types to show
        /// </summary>
        public JsonType ContentType { get; set; }

        /// <summary>
        /// WIP 16.08 - list the configuration types for a field.
        /// This is so the UI knows what metadata types to request when editing the field.
        /// </summary>
        public IDictionary<string, bool> ConfigTypes {get; set; }
    }
}
