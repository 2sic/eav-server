using ToSic.Eav.Sys;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class AdamItemDto
{
    /// <summary>
    /// Optional error message, should normally be null if no error
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    /// <summary>
    /// The file name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// This contains the code like "file:2742"
    /// </summary>
    public string? ReferenceId { get; init; }

    /// <summary>
    /// Normal url to access the resource
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>
    /// The Adam type, such as "folder", "image" etc.
    /// </summary>
    public string? Type { get; init; }

    public bool IsFolder { get; }
    public bool AllowEdit { get; init; }
    public int Size { get; init; }

    /// <summary>
    /// The Metadata for this ADAM item
    /// </summary>
    public IEnumerable<AdamMetadataOfDto>? Metadata { get; init; }

    public string? Path { get; set; }

    public DateTime Created { get; }
    public DateTime Modified { get; }

    /// <summary>
    /// Small preview thumbnail
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Large preview
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PreviewUrl { get; init; }

    public AdamItemDto(string error) => Error = error;

    public AdamItemDto(bool isFolder, string name, int size, DateTime created, DateTime modified)
    {
        IsFolder = isFolder;
        // note that the type will be set by other code later on if it's a file
        Type = isFolder ? "folder" : EavConstants.NullNameId;
        Name = name;
        Size = size;
        Created = created;
        Modified = modified;
    }

}