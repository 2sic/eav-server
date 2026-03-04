namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal assembler to assemble content types and content type attributes.
/// </summary>
/// <param name="contentTypeBuilder"></param>
/// <param name="typeAttributeBuilder"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypeAssembler(
    LazySvc<ContentTypeTypeAssembler> contentTypeBuilder,
    LazySvc<ContentTypeAttributeAssembler> typeAttributeBuilder)
    : ServiceWithSetup<DataAssemblerOptions>("DaB.CtAss", connect: [contentTypeBuilder, typeAttributeBuilder])
{
    public ContentTypeTypeAssembler Type => contentTypeBuilder.Value;

    public ContentTypeAttributeAssembler Attribute => typeAttributeBuilder.Value;

}