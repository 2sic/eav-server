﻿using ToSic.Lib.Data;

namespace ToSic.Sys.Capabilities.Aspects;

/// <summary>
/// Base class for various aspects of the system, such as features or capabilities.
/// </summary>
[PrivateApi("no good reason to publish this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record Aspect(): IHasIdentityNameId
{
    public const string PatronsUrl = "https://patrons.2sxc.org";

    protected Aspect(string NameId, Guid Guid, string Name, string? Description = default): this()
    {
        this.NameId = NameId;
        this.Guid = Guid;
        this.Name = Name;
        this.Description = Description ?? "";
    }

    // ReSharper disable InconsistentNaming
    public static Aspect Custom(string NameId, Guid Guid, string? Name = default, string? Description = default)
        => new() { NameId = NameId, Guid = Guid, Name = Name ?? NameId, Description = Description ?? "" };
    // ReSharper restore InconsistentNaming

    public static Aspect None = new() { NameId = "None", Guid = Guid.Empty, Name = "None" };

    /// <summary>
    /// GUID Identifier for this Aspect.
    /// </summary>
    public required Guid Guid { get; init; }

    /// <summary>
    /// String Identifier for this Aspect.
    /// </summary>
    public virtual required string NameId { get; init; }

    /// <summary>
    /// A nice name / title for showing in UIs
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A nice description
    /// </summary>
    public string Description { get; init; } = "";

    public override string ToString() => $"Aspect: {Name} ({NameId} / {Guid})";
}