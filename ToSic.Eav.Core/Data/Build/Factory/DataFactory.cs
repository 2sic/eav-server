﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static System.StringComparer;

namespace ToSic.Eav.Data.Build
{
    [PrivateApi("Still experimental/hide implementation")]
    public class DataFactory : ServiceBase, IDataFactory
    {
        #region Constructor / DI


        /// <summary>
        /// Constructor for DI
        /// </summary>
        public DataFactory(DataBuilder builder) : base("Ds.DatBld")
        {
            ConnectServices(
                _builder = builder
            );
        }
        private readonly DataBuilder _builder;

        #endregion


        #region Properties to configure Builder / Defaults

        /// <inheritdoc />
        public int AppId { get; private set; } = DataConstants.DataFactoryDefaultAppId;

        /// <inheritdoc />
        public string TitleField { get; private set; } = Attributes.TitleNiceName;

        /// <inheritdoc />
        public int IdCounter { get; private set; }

        /// <inheritdoc />
        public IContentType ContentType { get; private set; }

        /// <inheritdoc />
        public bool IdAutoIncrementZero { get; private set; }


        public DateTime Created { get; } = DateTime.Now;
        public DateTime Modified { get; } = DateTime.Now;

        private CreateFromNewOptions CreateFromNewOptions { get; set; }

        public ILookup<object, IEntity> Relationships => _nonLazyRelationships ?? _lazyRelationships;
        private ILookup<object, IEntity> _nonLazyRelationships;
        private LazyLookup<object, IEntity> _lazyRelationships;
        #endregion


        #region Configure
        /// <inheritdoc />
        public IDataFactory Configure(
            string noParamOrder = Parameters.Protector,
            int appId = default,
            string typeName = default,
            string titleField = default,
            int idSeed = DataConstants.DataFactoryDefaultIdSeed,
            bool idAutoIncrementZero = true,
            ILookup<object, IEntity> relationships = default,
            CreateFromNewOptions createFromNewOptions = default
        )
        {
            // Ensure parameters are named
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder, nameof(Configure));

            // Prevent the developer from re-using the DataFactory
            if (_alreadyConfigured)
                throw new Exception(
                    $"{nameof(Configure)} was already called - you cannot call it twice. " +
                    $"To get another {nameof(IDataFactory)}, use Dependency Injection and/or a Generator<{nameof(IDataFactory)}>.");
            _alreadyConfigured = true;

            // Store settings
            AppId = appId;
            TitleField = titleField.UseFallbackIfNoValue(Attributes.TitleNiceName);
            IdCounter = idSeed;
            ContentType = _builder.ContentType.Transient(typeName ?? DataConstants.DataFactoryDefaultTypeName);
            IdAutoIncrementZero = idAutoIncrementZero;

            CreateFromNewOptions = createFromNewOptions ?? new CreateFromNewOptions();

            // Determine what relationships source to use
            // If we got a lazy, use that and mark as lazy
            // If we got a normal one, preserve it as it should be the master and not use the lazy ones
            // which must be created anyway to avoid errors in later code
            var relationshipsAsLazy = relationships as LazyLookup<object, IEntity>;
            _lazyRelationships = relationshipsAsLazy ?? new LazyLookup<object, IEntity>();
            if (relationshipsAsLazy == null && relationships != null)
                _nonLazyRelationships = relationships;

            return this;
        }
        private bool _alreadyConfigured;
        #endregion

        #region Build / Finalize

        /// <inheritdoc />
        public IImmutableList<IEntity> Create<T>(IEnumerable<T> list) where T : IRawEntity
            => WrapUp(Prepare(list));

        /// <inheritdoc />
        public IImmutableList<IEntity> Create<T>(IEnumerable<IHasRawEntity<T>> list) where T : IRawEntity
            => WrapUp(Prepare(list));

        /// <inheritdoc />
        public IImmutableList<IEntity> WrapUp(IEnumerable<ICanBeEntity> rawList) => Log.Func(l =>
        {
            // Pre-process relationship keys, so they are added to the lookup
            var list = rawList.ToList();
            var itemsWithKeys = list
                .Where(item => item is IEntityPair<IRawEntity>)
                .Cast<IEntityPair<IRawEntity>>()
                .Select(pair => new EntityPair<IHasRelationshipKeys>(pair.Entity, pair.Partner as IHasRelationshipKeys))
                .Where(x => x.Partner?.RelationshipKeys?.Any() == true)
                .ToList();
            var keyMap = itemsWithKeys
                .SelectMany(pair => pair.Partner.RelationshipKeys
                    .Select(rk => new KeyValuePair<object, IEntity>(rk, pair.Entity)))
                .ToList();
            if (keyMap.Any()) _lazyRelationships.Add(keyMap);

            l.A($"Found {nameof(itemsWithKeys)} {itemsWithKeys.Count()}");

            // Return entities as Immutable list
            return list.Select(set => set.Entity).ToImmutableList();
        });

        #endregion

        #region Prepare One

        /// <inheritdoc />
        public EntityPair<T> Prepare<T>(IHasRawEntity<T> withRawEntity) where T: IRawEntity
            => Prepare<T>(withRawEntity.RawEntity);

        /// <inheritdoc />
        public EntityPair<T> Prepare<T>(T rawEntity) where T : IRawEntity
            => new EntityPair<T>(CreateFromRaw(rawEntity), rawEntity);

        #endregion


        #region Prepare Many

        /// <inheritdoc />
        public IList<EntityPair<T>> Prepare<T>(IEnumerable<IHasRawEntity<T>> data) where T: IRawEntity
            => data.Select(Prepare).ToList();

        /// <inheritdoc />
        public IList<EntityPair<TNewEntity>> Prepare<TNewEntity>(IEnumerable<TNewEntity> list) where TNewEntity : IRawEntity
        {
            var all = list.Select(n =>
                {
                    IEntity newEntity = null;

                    // Todo: improve this, so if anything fails, we have a clear info which item failed
                    try
                    {
                        newEntity = CreateFromRaw(n);
                        return new EntityPair<TNewEntity>(newEntity, n);
                    }
                    catch
                    {
                        /* ignore */
                    }

                    return new EntityPair<TNewEntity>(newEntity, n);
                })
                .ToList();
            return all;
        }

        #endregion

        #region Create

        /// <inheritdoc />
        public IEntity Create(Dictionary<string, object> values,
            int id = default,
            Guid guid = default,
            DateTime created = default,
            DateTime modified = default)
        {
            // pre-process RawRelationships
            values = values ?? new Dictionary<string, object>();
            var valuesWithRelationships = PreConvertRelationships(values);

            var ent = _builder.Entity.Create(
                appId: AppId,
                entityId: id == 0 && IdAutoIncrementZero ? IdCounter++ : id,
                contentType: ContentType,
                attributes: _builder.Attribute.Create(/*values*/valuesWithRelationships),
                titleField: TitleField,
                guid: guid,
                created: created == default ? Created : created,
                modified: modified == default ? Modified : modified
            );
            return ent;
        }

        private Dictionary<string, object> PreConvertRelationships(Dictionary<string, object> values)
        {
            var valuesWithRelationships = values.ToDictionary(
                v => v.Key,
                v =>
                {
                    if (!(v.Value is RawRelationship rawRelationship)) return v.Value;
                    var lookupSource =
                        new LookUpEntitiesSource<object>(rawRelationship.Keys, Relationships);
                    var relAttr = _builder.Attribute.CreateOneWayRelationship(v.Key, lookupSource);
                    return relAttr;
                }, InvariantCultureIgnoreCase);
            return valuesWithRelationships;
        }

        /// <summary>
        /// Internal create from raw
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <returns></returns>
        private IEntity CreateFromRaw(IRawEntity rawEntity) => Create(
            rawEntity.GetProperties(CreateFromNewOptions),
            id: rawEntity.Id,
            guid: rawEntity.Guid,
            created: rawEntity.Created,
            modified: rawEntity.Modified
        );

        #endregion

        #region Relationships


        #endregion
    }
}
