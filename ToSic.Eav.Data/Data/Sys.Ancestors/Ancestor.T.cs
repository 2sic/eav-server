﻿using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Sys.Ancestors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class Ancestor<T>(IAppIdentity parent, int id) : AppIdentity(parent), IAncestor, IDecorator<T>
{
    public int Id { get; set;  } = id;
}