using System;
using ToSic.Lib.Coding;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.SysData;

/// <summary>
/// Base class for various aspects of the system, such as features or capabilities.
/// </summary>
[PrivateApi("no good reason to publish this")]
public class Aspect: IHasIdentityNameId
{
    public const string PatronsUrl = "https://patrons.2sxc.org";

    protected Aspect(string nameId, Guid guid, string name, string description = default)
    {
        NameId = nameId;
        Guid = guid;
        Name = name;
        Description = description;
    }

    public static Aspect Custom(string nameId, Guid guid, string name = default, string description = default) =>
        new(nameId, guid, name, description);

    public static Aspect None = new("None", Guid.Empty, "None");

    /// <summary>
    /// GUID Identifier for this Aspect.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// String Identifier for this Aspect.
    /// </summary>
    public string NameId { get; }

    /// <summary>
    /// A nice name / title for showing in UIs
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A nice description
    /// </summary>
    public string Description { get; }

    public virtual Aspect Clone(StableApi.NoParamOrder noParamOrder = default, string nameId = default,
        Guid? guid = default, string name = default, string description = default) => new(nameId ?? NameId,
        guid ?? Guid, name ?? Name, description ?? Description);


    public override string ToString() => $"Aspect: {Name} ({NameId} / {Guid})";
}