using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data.Raw;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A data builder which will generate items for a specific type.
    /// In many cases it will also take care of auto increasing the id and more.
    ///
    /// This is more efficient than using the bare bones <see cref="IDataBuilderInternal"/>
    /// </summary>
    /// <remarks>
    /// * Added in v15 to replace the previous IDataBuilder which is now internal
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("in development for v15")]
    public interface IDataBuilder
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
        /// <param name="createRawOptions">Optional special options which create-raw might use</param>
        /// <returns>Itself, to make call chaining easier</returns>
        IDataBuilder Configure(
            string noParamOrder = Parameters.Protector,
            int appId = default,
            string typeName = default,
            string titleField = default,
            int idSeed = DataBuilder.DefaultIdSeed,
            bool idAutoIncrementZero = true,
            CreateRawOptions createRawOptions = default
        );

        /// <summary>
        /// For objects which delegate the IRawEntity to a property.
        /// </summary>
        /// <param name="withRawEntity"></param>
        /// <returns></returns>
        IEntity Create(IHasRawEntity withRawEntity);

        /// <summary>
        /// For objects which themselves are IRawEntity
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <returns></returns>
        IEntity Create(IRawEntity rawEntity);

        IEntity Create(Dictionary<string, object> values,
            int id = default,
            Guid guid = default,
            DateTime created = default,
            DateTime modified = default);

        IImmutableList<IEntity> CreateMany(IEnumerable<IRawEntity> rawEntities);
    }
}