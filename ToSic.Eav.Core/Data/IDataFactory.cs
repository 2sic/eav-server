using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data.New;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Factory
{
    /// <summary>
    /// A data builder which will generate items for a specific type.
    /// In many cases it will also take care of auto increasing the id and more.
    /// </summary>
    /// <remarks>
    /// * Added in v15 to replace the previous IDataFactory which is now internal
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("in development for v15 - should be final soon")]
    public interface IDataFactory
    {
        /// <summary>
        /// The App-ID which will be assigned to the generated entities.
        /// By default it will be `0`
        /// </summary>
        int AppId { get; }

        /// <summary>
        /// The field in the data which is the default title.
        /// Defaults to `Title` if not set.
        /// </summary>
        string TitleField { get; }

        /// <summary>
        /// A counter for the ID in case the data provided doesn't have an ID to use.
        /// Default is `1`
        /// </summary>
        int IdCounter { get; }

        /// <summary>
        /// Determines if Zero IDs are auto-incremented - default is `true`.
        /// </summary>
        bool IdAutoIncrementZero { get; }


        /// <summary>
        /// The generated ContentType.
        /// This will only be generated once, for better performance.
        /// </summary>
        IContentType ContentType { get; }

        /// <summary>
        /// Initial configuration call to setup all the parameters for this builder.
        /// It must be called before building anything. 
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="appId">The App this is virtually coming from, defaults to `0`</param>
        /// <param name="typeName">The name of the virtual content-type, defaults to `unspecified`</param>
        /// <param name="titleField">The field name to use as title, defaults to `Title`</param>
        /// <param name="idSeed">Default is `1`</param>
        /// <param name="idAutoIncrementZero">Default is `true`</param>
        /// <param name="createFromNewOptions">Optional special options which create-raw might use</param>
        /// <returns>Itself, to make call chaining easier</returns>
        IDataFactory Configure(
            string noParamOrder = Parameters.Protector,
            int appId = default,
            string typeName = default,
            string titleField = default,
            int idSeed = DataFactory.DefaultIdSeed,
            bool idAutoIncrementZero = true,
            CreateFromNewOptions createFromNewOptions = default
        );

        #region Build / Finalize

        /// <summary>
        /// Build a complete stream of <see cref="INewEntity"/>s.
        /// This is the method to use when you don't plan on doing any post-processing.
        ///
        /// If you need post-processing, call `Prepare` instead and finish using `Finalize`.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        IImmutableList<IEntity> Build<T>(IEnumerable<T> list) where T : INewEntity;

        /// <summary>
        /// Build a complete stream of <see cref="INewEntity"/>s.
        /// This is the method to use when you don't plan on doing any post-processing.
        ///
        /// If you need post-processing, call `Prepare` instead and finish using `Finalize`.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        IImmutableList<IEntity> Build<T>(IEnumerable<IHasNewEntity<T>> list) where T: INewEntity;

        /// <summary>
        /// Finalize the work of building something, using prepared materials.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        IImmutableList<IEntity> Finalize(IEnumerable<ICanBeEntity> list);

        #endregion

        #region Prepare One

        /// <summary>
        /// For objects which delegate the <see cref="INewEntity"/> to a property.
        /// </summary>
        /// <param name="withNewEntity"></param>
        /// <returns></returns>
        NewEntitySet<T> Prepare<T>(IHasNewEntity<T> withNewEntity) where T : INewEntity;

        /// <summary>
        /// For objects which themselves are <see cref="INewEntity"/>
        /// </summary>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        NewEntitySet<T> Prepare<T>(T newEntity) where T : INewEntity;

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
        /// <param name="list"></param>
        /// <returns></returns>
        IList<NewEntitySet<T>> Prepare<T>(IEnumerable<IHasNewEntity<T>> data) where T : INewEntity;

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
        IList<NewEntitySet<T>> Prepare<T>(IEnumerable<T> list) where T: INewEntity;

        #endregion

        IEntity Create(Dictionary<string, object> values,
            int id = default,
            Guid guid = default,
            DateTime created = default,
            DateTime modified = default);




    }
}