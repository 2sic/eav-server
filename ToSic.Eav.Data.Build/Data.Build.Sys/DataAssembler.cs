namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal data assembler to create entities, relationships, attributes and values.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class DataAssembler(
    Generator<EntityAssembler, DataAssemblerOptions> entityAssembler,
    Generator<EntityConnectionBuilder, DataAssemblerOptions> entityConnectionBuilder,
    Generator<AttributeAssembler, DataAssemblerOptions> attributeAssembler,
    Generator<AttributeListAssembler, DataAssemblerOptions> attributeListAssembler,
    Generator<RelationshipAssembler, DataAssemblerOptions> relationshipAssembler)
    : ServiceWithSetup<DataAssemblerOptions>("DaB.MltBld", connect:
        [
            entityAssembler, entityConnectionBuilder, attributeAssembler, attributeListAssembler, relationshipAssembler
        ])
{
    protected override DataAssemblerOptions GetDefaultOptions() => new();

    public EntityAssembler Entity => field ??= entityAssembler.New(MyOptions);

    public EntityConnectionBuilder EntityConnection => field ??= entityConnectionBuilder.New(MyOptions);

    [field: AllowNull, MaybeNull]
    public AttributeAssembler Attribute => field ??= attributeAssembler.New(MyOptions);

    [field: AllowNull, MaybeNull]
    public AttributeListAssembler AttributeList => field ??= attributeListAssembler.New(MyOptions);

    [field: AllowNull, MaybeNull]
    public ValueAssembler Value => field ??= Attribute.Values;

    [field: AllowNull, MaybeNull]
    public ValueListAssembler ValueList => field ??= Attribute.ValueList;

    public RelationshipAssembler Relationship => field ??= relationshipAssembler.New(MyOptions);

    public LanguageAssembler Language => field ??= Attribute.Languages;

}