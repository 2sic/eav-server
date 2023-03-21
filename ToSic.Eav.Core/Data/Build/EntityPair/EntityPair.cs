﻿using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Build
{
    /// <summary>
    /// Special object to carry an IEntity and another object which belong together.
    ///
    /// This is mainly used in scenarios where you create new Entities and must keep the original tied to the Entity
    /// because you may still need the original later on for further processing. 
    /// </summary>
    /// <typeparam name="TPartner"></typeparam>
    /// <remarks>
    /// Added in 15.04
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public class EntityPair<TPartner>: IEntityPair<TPartner>
    {
        public EntityPair(IEntity entity, TPartner partner)
        {
            Partner = partner;
            Entity = entity;
        }

        /// <summary>
        /// The partner object.
        /// For example an <see cref="IRawEntity"/> which was used to create this entity.
        /// </summary>
        public TPartner Partner { get; }

        /// <summary>
        /// The entity.
        /// </summary>
        public IEntity Entity { get; }
    }
}