using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

/// <summary>
/// Used to deserialize 'editions' from app.json
/// </summary>
public class AppJson
{
    public bool IsConfigured { get; set; }
    public ExportConfig Export { get; set; }
    public DotNetConfig DotNet { get; set; }
    public Dictionary<string, EditionInfo> Editions { get; set; } = [];
}

public class ExportConfig
{
    public List<string> Exclude { get; set; }
}

public class DotNetConfig
{
    public string RazorCompiler { get; set; }
}

public class EditionInfo(string description)
{
    public string Description { get; set; } = description;
    public bool IsDefault { get; set; }
}
