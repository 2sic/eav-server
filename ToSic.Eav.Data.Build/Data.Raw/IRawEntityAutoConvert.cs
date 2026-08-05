using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Raw;

/// <summary>
/// AutoConvert using <see cref="RawFromAnonymousHelper"/>
/// </summary>
/// <remarks>
/// Only supports fully automatic conversion, without relationships etc.
/// </remarks>
public interface IRawEntityAutoConvert : IRawEntitySource;
