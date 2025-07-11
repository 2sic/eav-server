﻿using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    #region Many variations of the Convert-Statement expecting IEntity

    /// <inheritdoc/>
    public IEnumerable<EavLightEntity> Convert(IEnumerable<IEntity> entities)
    {
        var l = Log.Fn<IEnumerable<EavLightEntity>>(timer: true);
        var topEntities = MaxItems == 0
            ? entities
            : entities.Take(MaxItems);
        var result = topEntities
            .Select(GetDictionaryFromEntity)
            .ToListOpt();
        return l.ReturnAsOk(result);
    }

    /// <inheritdoc/>
    public EavLightEntity Convert(IEntity entity)
    {
        var l = Log.Fn<EavLightEntity>(timer: true);
        var result = entity == null!
            ? null!
            : GetDictionaryFromEntity(entity);
        return l.ReturnAsOk(result);
    }

    #endregion

    #region Dynamic/Objects - which first must detect what it is

    public IEnumerable<EavLightEntity> Convert(IEnumerable<object> list)
    {
        // TODO CONTINUE HERE
        // MUST TEST EVERYTHING


        // 2021-09-11 DataStreams can be handled a bit more efficiently
        if (list is IEnumerable<IEntity> stream)
            return Convert(stream);

        return list
            .Select(c =>
            {
                IEntity? entity = null;
                if (c is IEntity ent)
                    entity = ent;
                else if (c is ICanBeEntity dynEnt)
                    entity = dynEnt.Entity;
                if (entity == null)
                    throw new("tried to convert an item, but it was not a known Entity-type");
                return GetDictionaryFromEntity(entity);
            })
            .ToListOpt();
    }

    public virtual EavLightEntity Convert(object item)
        => item switch
        {
            IEntity ent => Convert(ent),
            ICanBeEntity dynEnt => Convert(dynEnt.Entity),
            _ => throw new ArgumentException(@"expected an IEntity or IEntityWrapper like a DynamicEntity, but got something else", nameof(item))
        };

    #endregion

    public IEnumerable<EavLightEntity> Convert(IEnumerable<IEntityWrapper> list)
        => Convert(list as IEnumerable<object>);

    /// <inheritdoc />
    public EavLightEntity Convert(IEntityWrapper item)
        => Convert(item.Entity);
}