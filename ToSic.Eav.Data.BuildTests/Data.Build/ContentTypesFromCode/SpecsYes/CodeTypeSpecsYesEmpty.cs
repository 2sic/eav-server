using ToSic.Eav.Data.ContentTypes;
using static ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes.CodeTypeSpecsConstants;

namespace ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes;

/// <summary>
/// Just an empty content type with specs.
/// The specs are constants, as we'll reuse them in all CodeType With Specs
/// </summary>
[ContentType(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public class CodeTypeSpecsYesEmpty;