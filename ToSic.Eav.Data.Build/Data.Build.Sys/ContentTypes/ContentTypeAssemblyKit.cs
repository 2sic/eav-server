namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal assembler to assemble content types and content type attributes.
/// </summary>
/// <param name="contentTypeBuilder"></param>
/// <param name="typeAttributeBuilder"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypeAssemblyKit(
    LazySvc<ContentTypeAssembler> contentTypeBuilder,
    LazySvc<ContentTypeAttributeAssembler> typeAttributeBuilder)
    : ServiceWithSetup<DataAssemblerOptions>("DaB.CtAss", connect: [contentTypeBuilder, typeAttributeBuilder])
{
    public ContentTypeAssembler Type => contentTypeBuilder.Value;

    public ContentTypeAttributeAssembler Attribute => typeAttributeBuilder.Value;

}