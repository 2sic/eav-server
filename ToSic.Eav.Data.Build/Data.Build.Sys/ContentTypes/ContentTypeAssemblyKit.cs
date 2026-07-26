namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal assembler to assemble content types and content type attributes.
/// </summary>
/// <param name="contentTypeBuilder"></param>
/// <param name="fieldBuilder"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypeAssemblyKit(
    LazySvc<ContentTypeAssembler> contentTypeBuilder,
    LazySvc<ContentTypeFieldAssembler> fieldBuilder)
    : ServiceWithSetup<DataAssemblerOptions>("DaB.CtAss", connect: [contentTypeBuilder, fieldBuilder])
{
    public ContentTypeAssembler Type => contentTypeBuilder.Value;

    public ContentTypeFieldAssembler Field => fieldBuilder.Value;

}