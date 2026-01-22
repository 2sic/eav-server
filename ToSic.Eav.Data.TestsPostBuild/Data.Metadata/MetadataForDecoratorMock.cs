using ToSic.Eav.Metadata;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.Metadata;

/// <summary>
/// WIP v21: Decorator to provide Metadata access to decorated IEntity
/// </summary>
public class MetadataForDecoratorMock: IWrapperSetup<IEntity>
{
    //public static string ContentTypeNameId = "4c88d78f-5f3e-4b66-95f2-6d63b7858847";
    //public static string ContentTypeName = "MetadataForDecorator";

    void IWrapperSetup<IEntity>.SetupContents(IEntity source) => _entity = source;

    private IEntity _entity = null!;

    public int TargetType => _entity.Get(nameof(TargetType), fallback: (int)TargetTypes.None);

    public string TargetName => _entity.Get(nameof(TargetName), fallback: nameof(TargetTypes.None));

    public int Amount => _entity.Get(nameof(Amount), fallback: 1);

    public string? DeleteWarning => _entity.Get<string>(nameof(DeleteWarning), fallback: null);
}
