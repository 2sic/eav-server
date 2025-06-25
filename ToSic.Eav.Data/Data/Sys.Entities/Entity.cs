﻿using ToSic.Eav.Data.Sys.Attributes;

namespace ToSic.Eav.Data.Entities.Sys;

/// <summary>
/// A basic unit / item of data. Has many <see cref="IAttribute{T}"/>s which then contains <see cref="IValue{T}"/>s which are multi-language. 
/// </summary>
[PrivateApi("2021-09-30 hidden, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi, always use IEntity")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial record Entity: EntityLight, IEntity
{
    #region CanBeEntity

    IEntity ICanBeEntity.Entity => this;

    #endregion

    #region ToString to improve debugging experience

    public override string ToString() => $"{GetType()} =id:{EntityId}/{EntityGuid}";

    #endregion

    // #DropUseOfDumpProperties
    //[PrivateApi]
    //public List<PropertyDumpItem> _DumpNameWipDroppingMostCases(PropReqSpecs specs, string path)
    //    => new EntityDump().Dump(this, specs, path, null);
}