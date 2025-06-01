using ToSic.Eav.Metadata;

namespace ToSic.Metadata.Recommendations.Sys;

internal abstract class ForExpectedBase(IEntity entity) : EntityBasedType(entity)
{
    /// <summary>
    /// How may of this decorator should be applied to the target, default is 1
    /// </summary>
    public int Amount => GetThis(1);

    /// <summary>
    /// TargetType ID for what this is - so if we target Apps, Content-Types, Entities etc.
    /// </summary>
    public int TargetType => GetThis((int)TargetTypes.None);

    /// <summary>
    /// A delete warning if this is being deleted
    /// </summary>
    public string DeleteWarning => GetThis<string>(null);
}