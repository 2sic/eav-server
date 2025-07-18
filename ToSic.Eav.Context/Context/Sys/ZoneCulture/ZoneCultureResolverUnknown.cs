﻿namespace ToSic.Eav.Context.Sys.ZoneCulture;

internal sealed class ZoneCultureResolverUnknown: IZoneCultureResolver, IIsUnknown
{
    public ZoneCultureResolverUnknown(WarnUseOfUnknown<ZoneCultureResolverUnknown> _)
    {

    }

    public string DefaultCultureCode => "en-us";
    public string CurrentCultureCode => "en-us";
}