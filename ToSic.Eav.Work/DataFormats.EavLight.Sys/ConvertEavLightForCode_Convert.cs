using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.DataFormats.EavLight.Sys;

partial class ConvertEavLightForCode
{
    #region Many variations of the Convert-Statement expecting IEntity

    public IEnumerable<EavLightEntity> Convert(IEnumerable<IEntity> list) => convShared.Convert(list);

    public EavLightEntity Convert(IEntity item) => convShared.Convert(item);

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
                return convShared.GetDictionaryFromEntity(entity);
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

    IEnumerable<EavLightEntity> IConvertToEavLight.Convert(IEnumerable<IEntityWrapper> list)
        => convShared.Convert(list);

    EavLightEntity IConvertToEavLight.Convert(IEntityWrapper item)
        => convShared.Convert(item);

    #endregion

    IEnumerable<EavLightEntity> IConvert<IEntityWrapper, EavLightEntity>.Convert(IEnumerable<IEntityWrapper> list)
        => convShared.Convert(list);

    EavLightEntity IConvert<IEntityWrapper, EavLightEntity>.Convert(IEntityWrapper item)
        => convShared.Convert(item);
}