namespace ToSic.Sys.DI;

/// <summary>
/// Trivial marker record to add to the DI, so we can detect which keys were registered for a given service type.
/// This is useful for testing and diagnostics.
/// </summary>
/// <remarks>
/// Should remain internal, as it shouldn't be known outside the DI system.
/// </remarks>
/// <typeparam name="TService"></typeparam>
/// <param name="Key"></param>
internal record KeyMarker<TService>(string Key);