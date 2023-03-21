using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build
{
    [PrivateApi("hide implementation")]
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
        public int IdCounter { get; private set; }

        /// <inheritdoc />
        public IContentType ContentType { get; }

        public DataFactoryOptions Options { get; }


        public DateTime Created { get; } = DateTime.Now;
        public DateTime Modified { get; } = DateTime.Now;

        private RawConvertOptions RawConvertOptions { get; } = new RawConvertOptions();

        public ILookup<object, IEntity> Relationships => _nonLazyRelationships ?? _lazyRelationships;
        private ILookup<object, IEntity> _nonLazyRelationships;
        private LazyLookup<object, IEntity> _lazyRelationships = new LazyLookup<object, IEntity>();

        private RawRelationshipsConverter RelsConverter => _relsConverter.Get(() => new RawRelationshipsConverter(_builder, Log));
        private readonly GetOnce<RawRelationshipsConverter> _relsConverter = new GetOnce<RawRelationshipsConverter>();
        #endregion


        #region Spawn New

        public IDataFactory New(
            string noParamOrder = Parameters.Protector,
            int appId = default,
            string typeName = default,
            string titleField = default,
            int idSeed = DataConstants.DataFactoryDefaultIdSeed,
            bool idAutoIncrementZero = true,
            ILookup<object, IEntity> relationships = default,
            RawConvertOptions rawConvertOptions = default
        )
        {
            // Ensure parameters are named
            Parameters.Protect(noParamOrder);

            var clone = new DataFactory(_builder,
                options: new DataFactoryOptions(appId: appId, typeName: typeName, titleField: titleField, idSeed: idSeed, autoId: idAutoIncrementZero), relationships: relationships,
                rawConvertOptions: rawConvertOptions
                );
            if ((Log as Log)?.Parent != null) clone.LinkLog(((Log)Log).Parent);
            return clone;
        }

        public IDataFactory New(
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default,
            ILookup<object, IEntity> relationships = default,
            RawConvertOptions rawConvertOptions = default
        )
        {
            // Ensure parameters are named
            Parameters.Protect(noParamOrder);

            var clone = new DataFactory(_builder,
                options: options,
                relationships: relationships,
                rawConvertOptions: rawConvertOptions
            );
            if ((Log as Log)?.Parent != null) clone.LinkLog(((Log)Log).Parent);
            return clone;
        }

        /// <summary>
        /// Private constructor to create a new factory with configuration
        /// </summary>
        private DataFactory(
            DataBuilder builder,
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default,
            ILookup<object, IEntity> relationships = default,
            RawConvertOptions rawConvertOptions = default
        ) :this (builder)
        {
            // Ensure parameters are named
            Parameters.Protect(noParamOrder);

            // Store settings
            Options = options ?? new DataFactoryOptions();

            IdCounter = Options.IdSeed;
            ContentType = _builder.ContentType.Transient(Options.TypeName ?? DataConstants.DataFactoryDefaultTypeName);

            if (rawConvertOptions != null) RawConvertOptions = rawConvertOptions;

            // Determine what relationships source to use
            // If we got a lazy, use that and mark as lazy
            // If we got a normal one, preserve it as it should be the master and not use the lazy ones
            // which must be created anyway to avoid errors in later code
            if (relationships is LazyLookup<object, IEntity> relationshipsAsLazy)
                _lazyRelationships = relationshipsAsLazy;
            else
                _nonLazyRelationships = relationships;  // will be null or a real value
        }
        #endregion

        #region Create IRawEntity / WrapUp

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
            RelsConverter.AddRelationshipsToLookup(list, _lazyRelationships, RawConvertOptions);

            // Return entities as Immutable list
            return list.Select(set => set.Entity).ToImmutableList();
        });

        #endregion

        #region Prepare One

        /// <inheritdoc />
        public EntityPair<T> Prepare<T>(IHasRawEntity<T> withRawEntity) where T: IRawEntity
            => Prepare(withRawEntity.RawEntity);

        /// <inheritdoc />
        public EntityPair<T> Prepare<T>(T rawEntity) where T : IRawEntity
            => new EntityPair<T>(Create(rawEntity), rawEntity);

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
                        newEntity = Create(n);
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

        #region Create basic Dictionary

        /// <inheritdoc />
        public IEntity Create(Dictionary<string, object> values,
            int id = default,
            Guid guid = default,
            DateTime created = default,
            DateTime modified = default)
        {
            // pre-process RawRelationships
            values = values ?? new Dictionary<string, object>();
            var valuesWithRelationships = RelsConverter.RelationshipsToAttributes(values, Relationships);

            var ent = _builder.Entity.Create(
                appId: Options.AppId,
                entityId: id == 0 && Options.AutoId ? IdCounter++ : id,
                contentType: ContentType,
                attributes: _builder.Attribute.Create(valuesWithRelationships),
                titleField: Options.TitleField,
                guid: guid,
                created: created == default ? Created : created,
                modified: modified == default ? Modified : modified
            );
            return ent;
        }


        /// <summary>
        /// Internal create from raw
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <returns></returns>
        public IEntity Create(IRawEntity rawEntity) => Create(
            rawEntity.Attributes(RawConvertOptions),
            id: rawEntity.Id,
            guid: rawEntity.Guid,
            created: rawEntity.Created,
            modified: rawEntity.Modified
        );

        #endregion

    }
}
