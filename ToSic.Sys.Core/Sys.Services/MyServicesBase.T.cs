﻿#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Sys.Services;

/// <summary>
/// Special type of MyServices (dependency helpers).
/// This one is used to extend dependencies of a base classes <see cref="MyServicesBase{T}"/>.
/// They must then still have the original Dependencies to get them.
/// </summary>
/// <typeparam name="T"></typeparam>
[PublicApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MyServicesBase<T>(T parentServices, NoParamOrder protect = default, object[]? connect = default) : MyServicesBase(connect: connect)
{
    public T ParentServices { get; } = parentServices;

    // Note: don't add these to log queue, as they will be handled by the base class which needs these dependencies
}