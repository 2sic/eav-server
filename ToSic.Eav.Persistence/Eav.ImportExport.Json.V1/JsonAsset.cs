namespace ToSic.Eav.ImportExport.Json.V1;

/// <remarks>V 1.1</remarks>
public record JsonAsset
{
    /// <summary>
    /// Where the file should be placed - like in the app folder, in the app ADAM etc.
    /// ATM only "App" is implemented.
    /// Required ATM
    /// </summary>
    public string Storage { get; init; } = StorageApp;

    public const string StorageApp = "app";
    public static string[] Storages = [StorageApp];

    /// <summary>
    /// The file name, required.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The folder the file is in. Default is blank / null. Should always start with a folder name, not "/" or ".."
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public required string? Folder { get; init; }

    /// <summary>
    /// The file encoding - base64 or none for text/code files.
    /// Optional, defaults to none.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Encoding { get; init; } // = "none";

    public const string EncodingNone = "none";
    public const string EncodingBase64 = "base64";
    public static string[] Encodings = [EncodingNone, EncodingBase64];

    /// <summary>
    /// The file contents.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public required string? File { get; init; }

    /// <summary>
    /// File Metadata, if available
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public List<JsonEntity>? Metadata { get; init; }
}