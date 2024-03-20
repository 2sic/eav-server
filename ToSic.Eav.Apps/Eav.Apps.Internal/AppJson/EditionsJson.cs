namespace ToSic.Eav.Apps.Internal;

/// <summary>
/// Used to deserialize 'editions' from app.json
/// </summary>
public class EditionsJson
{
    public bool IsConfigured { get; set; }
    public Dictionary<string, EditionInfo> Editions { get; set; } = [];
}

public class EditionInfo(string description)
{
    public string Description { get; set; } = description;
    public bool IsDefault { get; set; }
}

