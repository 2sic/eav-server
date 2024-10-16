﻿using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.Internal.Unknown;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Apps.Integration;

internal sealed class AppInputTypesLoaderUnknown(WarnUseOfUnknown<AppInputTypesLoaderUnknown> _) : ServiceBase(LogConstants.FullNameUnknown), IAppInputTypesLoader, IIsUnknown
{
    // do nothing
    public IAppInputTypesLoader Init(IAppReader app) => this;

    public string Path { get; set; }

    public string PathShared { get; set; }

    // do nothing
    public List<InputTypeInfo> InputTypes() => [];
}