﻿using ToSic.Eav.Repositories;

namespace ToSic.Eav.Caching;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppLoaderTools
{
    IRepositoryLoader RepositoryLoader(ILog parentLog);
}