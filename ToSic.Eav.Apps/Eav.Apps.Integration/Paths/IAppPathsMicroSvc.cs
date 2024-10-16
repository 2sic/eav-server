﻿using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Integration;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppPathsMicroSvc
{
    IAppPaths Get(IAppReader appReader, ISite site = default);
}