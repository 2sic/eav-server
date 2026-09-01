using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes;

/// <summary>
/// Just an empty content type with specs.
/// The specs are constants, as we'll reuse them in all CodeType With Specs
/// </summary>
[ContentTypeUse(Type = typeof(CodeTypeSpecsYesEmpty))]
public class CodeTypeSpecsYesAssigned;