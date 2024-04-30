using System.Collections.Generic;
using ToSic.Lib.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps.Internal.Specs;

/// <summary>
/// Used to deserialize 'editions' from app.json
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppJson
{
    public bool IsConfigured { get; set; }
    public ExportConfig Export { get; set; }
    public DotNetConfig DotNet { get; set; }
    public Dictionary<string, EditionInfo> Editions { get; set; } = [];
}

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ExportConfig
{
    public List<string> Exclude { get; set; }
}

/// <summary>
/// DNN compiler set to roslyn
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DotNetConfig
{
    public string Compiler { get; set; }
}

/// <summary>
/// Used to deserialize 'editions' from app.json
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EditionInfo()
{
    public string Description { get; set; }
    public bool IsDefault { get; set; }
}
