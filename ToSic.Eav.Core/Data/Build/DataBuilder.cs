using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Data.New;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data
{
    [PrivateApi("Still experimental/hide implementation")]
    public class DataBuilder : ServiceBase, IDataBuilder
    {
        #region Properties to configure Builder / Defaults

        /// <inheritdoc />
        public int AppId { get; private set; } = DataBuilderInternal.DefaultAppId;

        /// <inheritdoc />
        public string TitleField { get; private set; } = Attributes.TitleNiceName;

        /// <inheritdoc />
        public int IdCounter { get; private set; }

        /// <inheritdoc />
        public IContentType ContentType { get; private set; }

        /// <inheritdoc />
        public bool IdAutoIncrementZero { get; private set; }

        public const int DefaultIdSeed = 1;

        public DateTime Created { get; } = DateTime.Now;
        public DateTime Modified { get; } = DateTime.Now;

        private CreateFromNewOptions CreateFromNewOptions { get; set; }

        #endregion

        #region Constructor / DI

        private readonly IDataBuilderInternal _parentBuilder;
        private readonly MultiBuilder _multiBuilder;

        /// <summary>
        /// Constructor for DI
        /// </summary>
        public DataBuilder(IDataBuilderInternal parentBuilder, MultiBuilder multiBuilder): base("Ds.DatBld")
        {
            ConnectServices(
                _parentBuilder = parentBuilder,
                _multiBuilder = multiBuilder
            );
        }

        #endregion

        /// <inheritdoc />
        public IDataBuilder Configure(
            string noParamOrder = Parameters.Protector,
            int appId = default,
            string typeName = default,
            string titleField = default,
            int idSeed = DefaultIdSeed,
            bool idAutoIncrementZero = true,
            CreateFromNewOptions createFromNewOptions = default
        )
        {
            // Ensure parameters are named
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder, nameof(Configure));

            // Prevent the developer from re-using the DataBuilder
            if (_alreadyConfigured)
                throw new Exception(
                    $"{nameof(Configure)} was already called - you cannot call it twice. " +
                    $"To get another {nameof(IDataBuilder)}, use Dependency Injection and/or a Generator<{nameof(IDataBuilder)}>.");
            _alreadyConfigured = true;

            // Store settings
            AppId = appId;
            TitleField = titleField.UseFallbackIfNoValue(Attributes.TitleNiceName);
            IdCounter = idSeed;
            ContentType = _parentBuilder.Type(typeName ?? DataBuilderInternal.DefaultTypeName);
            IdAutoIncrementZero = idAutoIncrementZero;

            CreateFromNewOptions = createFromNewOptions ?? new CreateFromNewOptions();
            return this;
        }
        private bool _alreadyConfigured;

        /// <inheritdoc />
        public IEntity Create(IHasNewEntity withNewEntity) => Create(withNewEntity.NewEntity);

        /// <inheritdoc />
        public IEntity Create(INewEntity newEntity) => Create(
            newEntity.GetProperties(CreateFromNewOptions),
            id: newEntity.Id, 
            guid: newEntity.Guid,
            created: newEntity.Created,
            modified: newEntity.Modified
        );

        public IImmutableList<IEntity> CreateMany(IEnumerable<INewEntity> rawEntities)
        {
            var all = rawEntities.Select(Create).ToList();
            return all.ToImmutableList();
        }

        public IDictionary<TRaw, IEntity> Prepare<TRaw>(IEnumerable<TRaw> rawEntities) where TRaw : INewEntity
        {
            var all = rawEntities.ToDictionary(r => r, r =>
            {
                
                // Todo: improve this, so if anything fails, we have a clear info which item failed
                try
                {
                    // WIP - this isn't nice ATM, but it's important
                    // so the resulting object has Multi-language attributes
                    // Should be improved ASAP
                    return _multiBuilder.FullClone(Create(r)) as IEntity;
                }
                catch
                {
                    return null;
                }
            });
            return all;
        }

        /// <inheritdoc />
        public IEntity Create(Dictionary<string, object> values,
            int id = default,
            Guid guid = default,
            DateTime created = default,
            DateTime modified = default)
        {
            var ent = _parentBuilder.Entity(values,
                appId: AppId,
                id: id == 0 && IdAutoIncrementZero ? IdCounter++ : id,
                type: ContentType,
                titleField: TitleField,
                guid: guid, 
                created: created == default ? Created : created,
                modified: modified == default ? Modified : modified
            );
            return ent;
        }
    }
}
