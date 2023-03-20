using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Process;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Build
{
    /// <summary>
    /// A data builder which will generate items for a specific type.
    /// In many cases it will also take care of auto increasing the id and more.
    /// </summary>
    /// <remarks>
    /// * Added in v15 to replace the previous IDataBuilder
    /// </remarks>
    [PublicApi]
    public interface IDataFactory
    {
        ///// <summary>
        ///// The App-ID which will be assigned to the generated entities.
        ///// By default it will be `0`
        ///// </summary>
        //int AppId { get; }

        ///// <summary>
        ///// The field in the data which is the default title.
        ///// Defaults to `Title` if not set.
        ///// </summary>
        //string TitleField { get; }

        /// <summary>
        /// A counter for the ID in case the data provided doesn't have an ID to use.
        /// Default is `1`
        /// </summary>
        int IdCounter { get; }

        ///// <summary>
        ///// Determines if Zero IDs are auto-incremented - default is `true`.
        ///// </summary>
        //bool IdAutoIncrementZero { get; }


        /// <summary>
        /// The generated ContentType.
        /// This will only be generated once, for better performance.
        /// </summary>
        IContentType ContentType { get; }

        /// <summary>
        /// TODO:
        /// </summary>
        ILookup<object, IEntity> Relationships { get; }

        /// <summary>
        /// Spawn a new <see cref="IDataFactory"/> with an initial configuration.
        /// This returns a _new_ <see cref="IDataFactory"/> and will not modify the original/parent.
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="appId">The App this is virtually coming from, defaults to `0`</param>
        /// <param name="typeName">The name of the virtual content-type, defaults to `unspecified`</param>
        /// <param name="titleField">The field name to use as title, defaults to `Title`</param>
        /// <param name="idSeed">Default is `1`</param>
        /// <param name="idAutoIncrementZero">Default is `true`</param>
        /// <param name="relationships"></param>
        /// <param name="rawConvertOptions">Optional special options which create-raw might use</param>
        /// <returns>Itself, to make call chaining easier</returns>
        IDataFactory New(
            string noParamOrder = Parameters.Protector,
            int appId = default,
            string typeName = default,
            string titleField = default,
            int idSeed = 1,
            bool idAutoIncrementZero = default,
            ILookup<object, IEntity> relationships = default,
            RawConvertOptions rawConvertOptions = default);

        IDataFactory New(
            string noParamOrder = Parameters.Protector,
            DataFactoryOptions options = default,
            ILookup<object, IEntity> relationships = default,
            RawConvertOptions rawConvertOptions = default);

        #region Simple Create

        /// <summary>
        /// Create a single entity based on values passed in.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="id"></param>
        /// <param name="guid"></param>
        /// <param name="created"></param>
        /// <param name="modified"></param>
        /// <returns></returns>
        IEntity Create(Dictionary<string, object> values,
            int id = default,
            Guid guid = default,
            DateTime created = default,
            DateTime modified = default);

        /// <summary>
        /// Create an entity from a single <see cref="IRawEntity"/>
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <returns></returns>
        IEntity Create(IRawEntity rawEntity);

        #endregion


        #region Create List

        /// <summary>
        /// Create a complete list of <see cref="IRawEntity"/>s.
        /// This is the method to use when you don't plan on doing any post-processing.
        ///
        /// If you need post-processing, call `Prepare` instead and finish using `WrapUp`.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        IImmutableList<IEntity> Create<T>(IEnumerable<T> list) where T : IRawEntity;

        /// <summary>
        /// Build a complete stream of <see cref="IRawEntity"/>s.
        /// This is the method to use when you don't plan on doing any post-processing.
        ///
        /// If you need post-processing, call `Prepare` instead and finish using `WrapUp`.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        IImmutableList<IEntity> Create<T>(IEnumerable<IHasRawEntity<T>> list) where T: IRawEntity;

        #endregion


        #region Prepare One

        /// <summary>
        /// For objects which delegate the <see cref="IRawEntity"/> to a property.
        /// </summary>
        /// <param name="withRawEntity"></param>
        /// <returns></returns>
        EntityPair<T> Prepare<T>(IHasRawEntity<T> withRawEntity) where T : IRawEntity;

        /// <summary>
        /// For objects which themselves are <see cref="IRawEntity"/>
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <returns></returns>
        EntityPair<T> Prepare<T>(T rawEntity) where T : IRawEntity;

        #endregion

        #region Prepare Many

        /// <summary>
        /// This will create IEntity but return it in a dictionary mapped to the original.
        /// This is useful when you intend to do further processing and need to know which original matches the generated entity.
        ///
        /// IMPORTANT: WIP
        /// THIS ALREADY RUNS FullClone, so the resulting IEntities are properly modifiable and shouldn't be cloned again
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        IList<EntityPair<T>> Prepare<T>(IEnumerable<IHasRawEntity<T>> data) where T : IRawEntity;

        /// <summary>
        /// This will create IEntity but return it in a dictionary mapped to the original.
        /// This is useful when you intend to do further processing and need to know which original matches the generated entity.
        ///
        /// IMPORTANT: WIP
        /// THIS ALREADY RUNS FullClone, so the resulting IEntities are properly modifiable and shouldn't be cloned again
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        IList<EntityPair<T>> Prepare<T>(IEnumerable<T> list) where T: IRawEntity;

        #endregion

        #region WrapUp

        /// <summary>
        /// Finalize the work of building something, using prepared materials.
        /// </summary>
        /// <param name="rawList"></param>
        /// <returns></returns>
        IImmutableList<IEntity> WrapUp(IEnumerable<ICanBeEntity> rawList);

        #endregion

    }
}