﻿namespace ToSic.Eav.Code;

/// <summary>
/// Interface to mark objects which can get a service.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanGetService
{
    /// <summary>
    /// Get a service from the Dependency Injection.
    /// The service can come from 2sxc, EAV or the underlying platform (Dnn, Oqtane).
    /// </summary>
    /// <typeparam name="TService">Interface (preferred) or Class which is needed</typeparam>
    /// <returns>An object of the type or interface requested, or null if not found in the DI.</returns>
    TService GetService<TService>() where TService : class;
}