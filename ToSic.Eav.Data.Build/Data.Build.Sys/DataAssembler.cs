namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal data assembler to create entities, relationships, attributes and values.
/// </summary>
/// <param name="entityBuilder"></param>
/// <param name="entityRelationshipBuilder"></param>
/// <param name="attributeAssembler"></param>
/// <param name="valueBuilder"></param>
/// <param name="languageAssembler"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class DataAssembler(
    LazySvc<EntityAssembler> entityBuilder,
    LazySvc<EntityConnectionBuilder> entityRelationshipBuilder,
    Generator<AttributeAssembler, DataAssemblerOptions> attributeAssembler,
    Generator<AttributeListAssembler, DataAssemblerOptions> attributeListAssembler,
    Generator<ValueAssembler, DataAssemblerOptions> valueBuilder,
    LanguageAssembler languageAssembler)
    : ServiceWithSetup<DataAssemblerOptions>("DaB.MltBld", connect:
        [
            entityBuilder, entityRelationshipBuilder, attributeAssembler, attributeListAssembler, valueBuilder, languageAssembler
        ])
{

    public EntityAssembler Entity => entityBuilder.Value;


    public EntityConnectionBuilder EntityConnection => entityRelationshipBuilder.Value;

    [field: AllowNull, MaybeNull]
    public AttributeAssembler Attribute => field
        ??= attributeAssembler.New(MyOptions);

    [field: AllowNull, MaybeNull]
    public AttributeListAssembler AttributeList => field
        ??= attributeListAssembler.New(MyOptions);


    [field: AllowNull, MaybeNull]
    public ValueAssembler Value => field
        ??= valueBuilder.New(MyOptions);

    [field: AllowNull, MaybeNull]
    public ValueListAssembler ValueList => field ??= new();

    public RelationshipAssembler Relationship => new();

    public LanguageAssembler Language => languageAssembler;
    
}