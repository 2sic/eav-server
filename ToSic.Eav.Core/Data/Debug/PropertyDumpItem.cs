﻿using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data.Debug
{
    [PrivateApi]
    public class PropertyDumpItem
    {
        public const string Separator = ".";

        internal static string[] BlacklistKeys = { "SettingsIdentifier", "ItemIdentifier" };


        public static bool ShouldStop(string path) => path?.Length > 200;

        public static PropertyDumpItem DummyErrorShouldStop(string path) => new PropertyDumpItem
        {
            Path = path + Separator + "ErrorTooDeep",
            Property = new PropertyRequest
            {
                FieldType = "Todo",
                Name = "error",
                Result = "error"
            }
        };

        /// <summary>
        /// The source of this item
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// The priority of this source - for proper sorting / priorities
        /// </summary>
        public int SourcePriority { get; set; }

        /// <summary>
        /// Path to this property
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Result data of the property
        /// </summary>
        public PropertyRequest Property { get; set; }

        public List<PropertyDumpItem> AllOptions { get; set; }
    }
}
