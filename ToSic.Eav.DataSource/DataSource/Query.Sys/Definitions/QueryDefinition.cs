using ToSic.Eav.Apps.Sys;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.Models;

namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// This contains the structure / definition of a query, which is internally stored in an <see cref="IEntity"/>
/// </summary>
/// <remarks>
/// Made visible in the docs v21.02, but still just fyi/internal.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public record QueryDefinition : ModelFromEntityBasic
{
    internal QueryDefinition(IEntity headerEntity, int appId, List<QueryPartDefinition> parts) : base(headerEntity)
    {
        AppId = appId;
        Parts = parts;
    }

    /// <summary>
    /// Content-Type name of the queryPart Content-Type
    /// </summary>
    internal static readonly string TypeName = "DataPipeline";

    /// <summary>
    /// The appid inside which the query will run, _not where it is stored!_ <br/>
    /// This can differ, because certain global queries (stored in the global app) will run in a specific app - for example to retrieve all ContentTypes of that app.
    /// </summary>
    public int AppId => field == KnownAppsConstants.AppIdEmpty
        ? Entity.AppId
        : field;

    public string Name => GetThis("error no name");

    /// <summary>
    /// The parts of the query
    /// </summary>
    public List<QueryPartDefinition> Parts { get; }


    /// <summary>
    /// Connections used in the query to map various DataSource Out-Streams to various other DataTarget In-Streams
    /// </summary>
    [field: AllowNull, MaybeNull]
    public IList<QueryWire> Connections => field ??= QueryWiringSerializer.Deserialize(StreamWiring);

    /// <summary>
    /// The connections as they are serialized in the Entity
    /// </summary>
    [PrivateApi]
    public string StreamWiring => GetThis("");

    #region Query Params

    /// <summary>
    /// The raw Params used in this query, as stored in the IEntity
    /// </summary>
    private string Params => GetThis<string>("");

    /// <summary>
    /// The param-dictionary used for the LookUp. All keys will be available in the token [Params:key]
    /// </summary>
    [field: AllowNull, MaybeNull]
    public IDictionary<string, string> ParamsDic
    {
        get => field ??= QueryDefinitionParams.GenerateParamsDic(Params, null);
        set;
    }

    /// <summary>
    /// The <see cref="ILookUp"/> for the params of this query - based on the Params.
    /// </summary>
    /// <returns>Always returns a valid ILookup, even if no params found. </returns>
    [field: AllowNull, MaybeNull]
    public ILookUp ParamsLookUp
    {
        get => field ??= new LookUpInDictionary(DataSourceConstants.ParamsSourceName, ParamsDic);
        set;
    }

    // #RemoveDataSourceReset v21
    /// <summary>
    /// Will reset all the parameters so you can run the query again with different parameters. 
    /// </summary>
    internal void Reset()
    {
        ParamsDic = null!;
        ParamsLookUp = null!;
    }

    #endregion

    #region Test Parameters

    /// <summary>
    /// The test parameters as stored in the IEntity
    /// </summary>
    public string? TestParameters => GetThis<string>(null);

    [PrivateApi]
    public List<ILookUp> TestParameterLookUps => QueryDefinitionParams.GenerateTestValueLookUps(TestParameters);

    #endregion
}