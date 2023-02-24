﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Data.New;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build
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

        #region Configure
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
                        newEntity = _multiBuilder.FullClone(Create(n));
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
