﻿using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataFormats.EavLight;
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
    }
}
