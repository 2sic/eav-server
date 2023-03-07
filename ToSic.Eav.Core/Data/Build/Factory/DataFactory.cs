using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Data.New;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Factory
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
            return this;
        }
        private bool _alreadyConfigured;
        #endregion

        #region Build / Finalize

        /// <inheritdoc />
        public IImmutableList<IEntity> Build<T>(IEnumerable<T> list) where T : INewEntity
            => Finalize(Prepare(list));

        /// <inheritdoc />
        public IImmutableList<IEntity> Build<T>(IEnumerable<IHasNewEntity<T>> list) where T : INewEntity
            => Finalize(Prepare(list));

        /// <inheritdoc />
        public IImmutableList<IEntity> Finalize(IEnumerable<ICanBeEntity> list)
            => list.Select(set => set.Entity).ToImmutableList();

        #endregion

        #region Prepare One

        /// <inheritdoc />
        public NewEntitySet<T> Prepare<T>(IHasNewEntity<T> withNewEntity) where T: INewEntity
            => Prepare<T>(withNewEntity.NewEntity);

        /// <inheritdoc />
        public NewEntitySet<T> Prepare<T>(T newEntity) where T : INewEntity
            => new NewEntitySet<T>(newEntity, Create(
                newEntity.GetProperties(CreateFromNewOptions),
                id: newEntity.Id,
                guid: newEntity.Guid,
                created: newEntity.Created,
                modified: newEntity.Modified
            ));

        #endregion


        #region Prepare Many

        /// <inheritdoc />
        public IList<NewEntitySet<T>> Prepare<T>(IEnumerable<IHasNewEntity<T>> data) where T: INewEntity
            => data.Select(Prepare).ToList();

        /// <inheritdoc />
        public IList<NewEntitySet<TNewEntity>> Prepare<TNewEntity>(IEnumerable<TNewEntity> list) where TNewEntity : INewEntity
        {
            var all = list.Select(n =>
                {
                    IEntity newEntity = null;

                    // Todo: improve this, so if anything fails, we have a clear info which item failed
                    try
                    {
                        // WIP - this isn't nice ATM, but it's important
                        // so the resulting object has Multi-language attributes
                        // Should be improved ASAP
                        newEntity = _builder.FullClone(Create(n));
                    }
                    catch
                    {
                        /* ignore */
                    }

                    return new NewEntitySet<TNewEntity>(n, newEntity);
                })
                .ToList();
            return all;
        }

        #endregion

        /// <inheritdoc />
        public IEntity Create(Dictionary<string, object> values,
            int id = default,
            Guid guid = default,
            DateTime created = default,
            DateTime modified = default)
        {
            var ent = _builder.Entity.Create(
                appId: AppId,
                entityId: id == 0 && IdAutoIncrementZero ? IdCounter++ : id,
                contentType: ContentType,
                attributes: _builder.Attribute.Create(values),
                titleField: TitleField,
                guid: guid,
                created: created == default ? Created : created,
                modified: modified == default ? Modified : modified
            );
            return ent;
        }

        #region Create internal

        private IEntity Create(INewEntity newEntity) => Create(
            newEntity.GetProperties(CreateFromNewOptions),
            id: newEntity.Id, 
            guid: newEntity.Guid,
            created: newEntity.Created,
            modified: newEntity.Modified
        );

        #endregion
    }
}
