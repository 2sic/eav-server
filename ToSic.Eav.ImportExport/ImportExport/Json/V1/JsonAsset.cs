using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    /// <remarks>V 1.1</remarks>
    public class JsonAsset
    {
        /// <summary>
        /// Where the file should be placed - like in the app folder, in the app ADAM etc.
        /// ATM only "App" is implemented.
        /// Required ATM
        /// </summary>
        public string Storage = StorageApp;

        public const string StorageApp = "app";
        public static string[] Storages = {StorageApp};

        /// <summary>
        /// The file name, required.
        /// </summary>
        public string Name;

        /// <summary>
        /// The folder the file is in. Default is blank / null. Should always start with a folder name, not "/" or ".."
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string Folder;

        /// <summary>
        /// The file encoding - base64 or none for text/code files.
        /// Optional, defaults to none.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string Encoding; // = "none";

        public const string EncodingNone = "none";
        public const string EncodingBase64 = "base64";
        public static string[] Encodings = {EncodingNone, EncodingBase64};

        /// <summary>
        /// The file contents.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string File;

        /// <summary>
        /// File Metadata, if available
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<JsonEntity> Metadata;
    }

}
