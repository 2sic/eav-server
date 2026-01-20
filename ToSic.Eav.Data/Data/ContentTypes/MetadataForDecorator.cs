using ToSic.Eav.Data.Wrap;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.ContentTypes;

/// <summary>
/// WIP v21: Decorator to provide Metadata access to decorated IEntity
/// </summary>
[PrivateApi("still WIP / internal")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MetadataForDecorator: ICanWrapEavBeta<IEntity>
{
    void ICanWrapEavBeta<IEntity>.Setup(IEntity source) => _entity = source;

    private IEntity _entity = null!;

    public int TargetType => _entity.Get(nameof(TargetType), fallback: (int)TargetTypes.None);

    public string TargetName => _entity.Get(nameof(TargetName), fallback: nameof(TargetTypes.None));

    public int Amount => _entity.Get(nameof(Amount), fallback: 1);

    public string? DeleteWarning => _entity.Get<string>(nameof(DeleteWarning), fallback: null);
}
