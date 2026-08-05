using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories.MockData;

/// <summary>
/// This test record claims to be convertible, but does not implement anything.
/// Trying to convert it will throw an exception.
/// </summary>
internal record MockRawConvertibleInvalid : IRawEntitySource;