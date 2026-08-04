//using ToSic.Eav.Metadata;

//namespace ToSic.Eav.Data.Raw.Sys;

///// <summary>
///// Record to provide a raw entity. It should never be inherited, as it's purpose is to bridge the gap between
///// PoCos and raw entity converters.
///// </summary>
///// <remarks>
///// Added in v22 - not sure if relevant, may remove again
///// </remarks>
//[PrivateApi]
//[ShowApiWhenReleased(ShowApiMode.Never)]
//public class RawEntityClassic: IRawEntity, IRelationshipKeys
//{
//    #region Core Properties Id, Guid, Created, Modified
    
//    public int Id
//    {
//        get;
//#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
//        set;
//#else
//        init;
//#endif
//    }

//    public Guid Guid
//    {
//        get;
//#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
//        set;
//#else
//        init;
//#endif
//    } = Guid.Empty;

//    public DateTime Created
//    {
//        get;
//#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
//        set;
//#else
//        init;
//#endif
//    } = DateTime.Now;

//    public DateTime Modified
//    {
//        get;
//#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
//        set;
//#else
//        init;
//#endif
//    } = DateTime.Now;

//    #endregion


//    #region Values

//    public IDictionary<string, object?> Values
//    {
//        get => field ??= GetValues();
//        init => field = value?.ToInvariant();
//    }

//    /// <summary>
//    /// Override this method to provide the values for the entity. By default, it returns an empty dictionary.
//    /// </summary>
//    /// <returns></returns>
//    protected virtual IDictionary<string, object?> GetValues() =>
//        new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);

//    #endregion

//    /// <inheritdoc/>
//    public IEnumerable<object>? RelationshipKeys { get; init; }

//    /// <summary>
//    /// WIP experimental v18.02 - trying to get content-type metadata into the raw entity
//    /// </summary>
//    public IMetadata? Metadata { get; init; }
//}
