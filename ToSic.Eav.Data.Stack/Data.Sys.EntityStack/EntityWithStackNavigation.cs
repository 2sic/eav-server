﻿using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.PropertyStack;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Data.Sys.EntityStack;

/// <summary>
/// This is a special IEntity-wrapper which will return Stack-Navigation
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class EntityWithStackNavigation(IEntity entity, StackAddress stackAddress)
    : EntityWrapper(entity), IPropertyStackLookup
{
    internal readonly PropertyStackNavigator Navigator = new(entity, stackAddress);

    public override PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
        => PropertyLookupWithStackNavigation.FindPropertyInternalOfStackWrapper(this, specs, path,
            EavLogs.Eav + ".EntNav", $"EntityId: {Entity?.EntityId}, Title: {Entity?.GetBestTitle()}");

    public PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path) 
        => Navigator.GetNextInStack(specs, startAtSource, path);
}