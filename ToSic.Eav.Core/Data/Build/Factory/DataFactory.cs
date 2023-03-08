using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
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

        public ILookup<string, IEntity> LookupWip { get; set; }

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
            //ILookup<string, IEntity> lookup = default,
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

            LookupWip = Enumerable.Empty<IEntity>().ToLookup(x => "", x => x);

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
        public IImmutableList<IEntity> WrapUp(IEnumerable<ICanBeEntity> list)
            => list.Select(set => set.Entity).ToImmutableList();

        #endregion

        #region Prepare One

        /// <inheritdoc />
        public EntityPair<T> Prepare<T>(IHasRawEntity<T> withRawEntity) where T: IRawEntity
            => Prepare<T>(withRawEntity.RawEntity);

        /// <inheritdoc />
        public EntityPair<T> Prepare<T>(T newEntity) where T : IRawEntity
            => new EntityPair<T>(Create(
                newEntity.GetProperties(CreateFromNewOptions),
                id: newEntity.Id,
                guid: newEntity.Guid,
                created: newEntity.Created,
                modified: newEntity.Modified
            ), newEntity);

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
            var valuesWithRelationships = values.ToDictionary(
                v => v.Key,
                v =>
                {
                    if (!(v.Value is RawRelationship rawRelationship)) return v.Value;
                    var lookupSource =
                        new LookUpEntitiesSource<string>(rawRelationship.Keys.ToImmutableList(), LookupWip);
                    var relAttr = _builder.Attribute.CreateOneWayRelationship(v.Key, lookupSource);
                    return relAttr;
                }, InvariantCultureIgnoreCase);

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

        #endregion

        #region Create internal

        private IEntity Create(IRawEntity rawEntity) => Create(
            rawEntity.GetProperties(CreateFromNewOptions),
            id: rawEntity.Id, 
            guid: rawEntity.Guid,
            created: rawEntity.Created,
            modified: rawEntity.Modified
        );

        #endregion

        #region Relationships

        public ILookup<string, IEntity> GenerateLookup(params IEnumerable<EntityPair<IRawEntity>>[] lists)
        {
            var pairs = lists.SelectMany(list =>
                list.SelectMany(pair =>
                    ((pair.Partner as IHasRelationshipKeys)?.RelationshipKeys ?? new List<string>())
                    .Select(rk => new EntityPair<string>(pair.Entity, rk))
                ));
            return pairs.ToLookup(pair => pair.Partner, pair => pair.Entity);
        }

        #endregion
    }
}
