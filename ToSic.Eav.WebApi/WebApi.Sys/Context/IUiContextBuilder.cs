﻿using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Context;

public interface IUiContextBuilder: IHasLog
{
    /// <summary>
    /// Initialize the context builder
    /// </summary>
    /// <returns></returns>
    IUiContextBuilder InitApp(IAppReader? appReaderOrNull);

    /// <summary>
    /// Get the context based on the situation
    /// </summary>
    /// <returns></returns>
    ContextDto Get(Ctx flags, CtxEnable enableFlags);
}