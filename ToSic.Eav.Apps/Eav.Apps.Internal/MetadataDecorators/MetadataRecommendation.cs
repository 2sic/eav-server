using System.Text.Json.Serialization;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

/// <summary>
/// Important: also used as DTO, so don't just rename the parameters
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MetadataRecommendation: IEquatable<MetadataRecommendation>
{
    public const int PrioMax = 100;
    public const int PrioLow = 1;
    public const int PrioMedium = 2;
    public const int PrioHigh = 10;

    public string Id => Type.NameId;

    public string Title { get; }

    public string Name => Type.Name;

    public int Count { get; set; }

    public string DeleteWarning { get; set; }

    public string Icon { get; set; }

    [JsonIgnore]
    public int Priority { get; set; }

    /// <summary>
    /// Marks the recommendation that it should be created as empty
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CreateEmpty { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Debug { get; set; }

    [JsonIgnore]
    public IContentType Type { get; }

    [JsonIgnore]
    internal bool PushToUi => Enabled || !string.IsNullOrWhiteSpace(MissingFeature);

    public bool Enabled { get; set; }
    public string MissingFeature { get; set; }

    internal MetadataRecommendation(IContentType type, MetadataForDecorator recommendation, int? count, string debugMessage, int priority)
    {
        Type = type;
        Priority = priority;
        var typeDetails = type.Metadata.DetailsOrNull;
        Title = (typeDetails?.Title).UseFallbackIfNoValue(type.Name);
        Icon = typeDetails?.Icon;
        var recDec = recommendation ?? new MetadataForDecorator(null);
        Count = count ?? recDec.Amount;
        Debug = debugMessage;
        DeleteWarning = recDec.DeleteWarning;

        // Mark empty if possible - so it has no attributes, and it has a decorator to support this
        if (!type.Attributes.Any() && type.Metadata.HasType(Metadata.Decorators.SaveEmptyDecoratorId))
            CreateEmpty = true;
    }

    #region Equality Comparison for deduplication
    public bool Equals(MetadataRecommendation other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MetadataRecommendation)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
        }
    }
    #endregion
}