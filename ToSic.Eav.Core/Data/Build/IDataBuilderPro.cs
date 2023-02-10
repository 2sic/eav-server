using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Raw;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A data builder which will generate items for a specific type.
    /// In many cases it will also take care of auto increasing the id and more.
    ///
    /// This is more efficient than using the bare bones <see cref="IDataBuilder"/>
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("in development for v15")]
    public interface IDataBuilderPro
    {
        /// <summary>
        /// The App-ID which will be assigned to the generated entities.
        /// By default it will be the same App as this was created on.
        /// </summary>
        int AppId { get; }

        /// <summary>
        /// The field in the data which is the default title.
        /// </summary>
        string TitleField { get; }

        /// <summary>
        /// A counter for the ID in case the data provided doesn't have an ID to use.
        /// </summary>
        int IdCounter { get; }

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
        /// <param name="appId"></param>
        /// <param name="typeName"></param>
        /// <param name="titleField"></param>
        /// <param name="idSeed"></param>
        /// <returns>Itself, to make call chaining easier</returns>
        IDataBuilderPro Configure(
            string noParamOrder = Parameters.Protector,
            int appId = DataBuilder.DefaultAppId,
            string typeName = default,
            string titleField = default,
            int idSeed = DataBuilderPro.DefaultIdSeed
        );

        /// <summary>
        /// For objects which delegate the IRawEntity to a property.
        /// </summary>
        /// <param name="withSource"></param>
        /// <returns></returns>
        IEntity Create(IHasRawEntitySource withSource);

        [PrivateApi("Todo - must find out what this is all about @STV?")] // TODO:
        IEntity CreateWithEavNullId(IRawEntity rawEntity);

        /// <summary>
        /// For objects which themselves are IRawEntity
        /// </summary>
        /// <param name="rawEntity"></param>
        /// <param name="nullId">when 0 is valid Id for some DataSources, provide Eav.Constants.NullId instead</param>
        /// <returns></returns>
        IEntity Create(IRawEntity rawEntity, int nullId = 0);

        IEntity Create(Dictionary<string, object> values, int? id = default, Guid? guid = default, DateTime? created = default, DateTime? modified = default);
    }
}