using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// Just an empty content type with specs.
/// The specs are constants, as we'll reuse them in all CodeType With Specs
/// </summary>
[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public class CodeTypeWithSpecsEmpty
{
    public const string SpecName = "TestTypeWithSpecsModified";
    public const string SpecGuid = "501ee043-1070-4cbc-a07b-8274f24bf5ea";
    public const string SpecScope = "DemoScope";
    public const string SpecDescription = "This is a test type with specs";
}