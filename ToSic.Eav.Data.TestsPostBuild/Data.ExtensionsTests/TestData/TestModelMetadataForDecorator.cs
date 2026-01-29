using ToSic.Eav.Metadata;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests.TestData;

/// <summary>
/// Test Sample Model
/// </summary>
public class TestModelMetadataForDecorator: IModelSetup<IEntity>
{
    //public static string ContentTypeNameId = "4c88d78f-5f3e-4b66-95f2-6d63b7858847";
    //public static string ContentTypeName = "MetadataForDecorator";

    bool IModelSetup<IEntity>.SetupModel(IEntity? source)
    {
        _entity = source!;
        return true;
    }

    private IEntity _entity = null!;

    public int TargetType => _entity.Get(nameof(TargetType), fallback: (int)TargetTypes.None);

    public string TargetName => _entity.Get(nameof(TargetName), fallback: nameof(TargetTypes.None));

    public int Amount => _entity.Get(nameof(Amount), fallback: 1);

    public string? DeleteWarning => _entity.Get<string>(nameof(DeleteWarning), fallback: null);
}

public class TestModelMetadataForDecoratorWrongName
    : TestModelMetadataForDecorator;

[ModelSpecs(ContentType = nameof(TestModelMetadataForDecorator))]
public class TestModelMetadataForDecoratorWithAttribute
    : TestModelMetadataForDecorator;