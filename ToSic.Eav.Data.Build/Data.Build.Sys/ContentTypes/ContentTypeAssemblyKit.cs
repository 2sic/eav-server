namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal assembler to assemble content types and content type attributes.
/// </summary>
/// <param name="contentTypeBuilder"></param>
/// <param name="fieldBuilder"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypeAssemblyKit(
    Generator<ContentTypeAssembler, DataAssemblerOptions> contentTypeBuilder,
    Generator<ContentTypeFieldAssembler, DataAssemblerOptions> fieldBuilder)
    : ServiceWithSetup<DataAssemblerOptions>("DaB.CtAss", connect: [contentTypeBuilder, fieldBuilder])
{
    protected override DataAssemblerOptions GetDefaultOptions() => new();
    
    public ContentTypeAssembler Type => field ??= contentTypeBuilder.New(MyOptions);

    public ContentTypeFieldAssembler Field => field ??= fieldBuilder.New(MyOptions);

}