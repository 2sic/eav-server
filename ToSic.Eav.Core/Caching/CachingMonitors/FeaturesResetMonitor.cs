﻿using System;
using System.Runtime.Caching;
using ToSic.Eav.Internal.Features;

namespace ToSic.Eav.Caching.CachingMonitors;

/// <summary>
/// Signal that some items in an app cache should be flushed
/// Idea from here: https://stackoverflow.com/questions/25269338/is-this-a-good-solution-to-clear-a-c-sharp-memorycache
/// </summary>
internal class FeaturesResetMonitor : ChangeMonitor
{
        
    internal FeaturesResetMonitor(IEavFeaturesService featuresService)
    {
        _featuresService = featuresService;
        _featuresService.FeaturesChanged += HandleFeaturesChanged;
        // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.caching.changemonitor?view=dotnet-plat-ext-6.0
        InitializationComplete(); // necessary for ChangeMonitors
    }
    private IEavFeaturesService _featuresService;

    ~FeaturesResetMonitor() => _featuresService.FeaturesChanged -= HandleFeaturesChanged;


    protected override void Dispose(bool disposing)
    {
        _featuresService.FeaturesChanged -= HandleFeaturesChanged;
        _featuresService = null;
        if (disposing) Dispose();
    }

    public override string UniqueId { get; } = Guid.NewGuid().ToString();

    public void HandleFeaturesChanged(object sender, EventArgs e)
    {
        if (_removed) return;
        _removed = true;
        // flush a cache and dispose ChangeMonitor
        this.OnChanged(null);
    }

    private bool _removed;
}