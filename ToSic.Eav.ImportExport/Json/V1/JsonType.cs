using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonType
    {
        /// <summary>
        /// Main static identifier, usually a guid, in rare cases a string such as "@string-dropdown"
        /// </summary>
        /// <remarks>
        /// Should kind of be NameId, but was created a long time ago, and a rename would cause too much trouble.
        /// </remarks>
        [JsonPropertyOrder(1)]
        public string Id { get; set; }

        /// <summary>
        /// Nice name, string
        /// </summary>
        [JsonPropertyOrder(2)]
        public string Name { get; set; }

        /// <summary>
        /// Map for attribute name to long-term-guid (if given)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyOrder(100)]
        public IDictionary<string, Guid> AttributeMap { get; set; }

        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        /// <summary>
        /// Additional description for the type, usually not included.
        /// Sometimes added in admin-UI scenarios, where additional info is useful
        /// ATM only used for Metadata
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Description { get; set; }

        /// <summary>
        /// Additional description for the type, usually not included.
        /// Sometimes added in admin-UI scenarios, where additional info is useful
        /// ATM only used for Metadata
        /// </summary>
        /// <remarks>Added in v13.02</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Title { get; set; }

        /// <summary>
        /// Empty constructor is important for de-serializing
        /// </summary>
        public JsonType() { }

        public JsonType(IEntity entity, bool withDescription = false, bool withMap = false)
        {
            var type = entity.Type;
            Name = type.Name;
            Id = type.NameId;
            if (withDescription)
            {
                var description = type.Metadata.DetailsOrNull;
                Title = description?.Title ?? type.NameId;
                Description = description?.Description ?? "";
            }

            if (withMap)
            {
                var withGuid = type.Attributes
                    .Where(a => a.Guid != null && a.Guid != Guid.Empty)
                    .ToDictionary(a => a.Name, a => a.Guid ?? Guid.Empty);
                if (withGuid.Any())
                    AttributeMap = withGuid;
            }
        }
    }
}
