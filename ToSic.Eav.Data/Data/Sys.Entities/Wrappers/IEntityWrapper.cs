﻿using ToSic.Lib.Wrappers;

namespace ToSic.Eav.Data.Sys.Entities;

/// <summary>
/// An interface to ensure all things that carry an IEntity can be compared based on the Entity they carry.
/// </summary>
[PrivateApi("Hide, was public before 2023-08-10")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IEntityWrapper
    : IHasDecorators<IEntity>,
        IMultiWrapper<IEntity>,
        ICanBeEntity;