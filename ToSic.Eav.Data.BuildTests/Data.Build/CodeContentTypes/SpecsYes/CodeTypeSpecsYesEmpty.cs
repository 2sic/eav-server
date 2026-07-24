using ToSic.Eav.Data.Sys.ContentTypes;
using static ToSic.Eav.Data.Build.CodeContentTypes.SpecsYes.CodeTypeSpecsConstants;

namespace ToSic.Eav.Data.Build.CodeContentTypes.SpecsYes;

/// <summary>
/// Just an empty content type with specs.
/// The specs are constants, as we'll reuse them in all CodeType With Specs
/// </summary>
[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public class CodeTypeSpecsYesEmpty;