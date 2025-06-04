
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using EavDbContext = ToSic.Eav.Persistence.Efc.Sys.DbContext.EavDbContext;

namespace ToSic.Eav.Persistence.Efc.Sys.Values;

internal class ValueQueriesPro(EavDbContext context, ILog parentLog): HelperBase(parentLog, "Efc.ValQry")
{
    internal (IQueryable<TsDynDataValue> Query, List<TsDynDataDimension> Dimensions) AllValuesQuery(int appId)
    {
        var l = Log.Fn<(IQueryable<TsDynDataValue>, List<TsDynDataDimension>)>(timer: true);

        var dimensions = context.TsDynDataDimensions.ToList(); // materialise (very fast)

        //var values = context.TsDynDataValues
        //        .Join(context.TsDynDataEntities, v => v.EntityId, e => e.EntityId, (v, e) => new { v, e })
        //        .Where(@t => @t.e.AppId == appId)
        //        .Select(@t => @t.v)
        //    .Include(v => v.TsDynDataValueDimensions);

        var values = context.TsDynDataValues
            .Where(@t => @t.Entity.AppId == appId)
            .Include(v => v.TsDynDataValueDimensions);

        return l.Return((values, dimensions), $"with {dimensions.Count} dimensions");
    }

    #region Test
    // 2025-05-12, stv, test alternative strategy to load values

    //internal IReadOnlyList<ValueResult> ValuesOfIdsQueryOptimized2(List<int> entityIds)
    //{
    //    // Build the table-valued parameter
    //    var tvp = BuildIntListParameter("@EntityIds", entityIds);

    //    // Set up ADO.NET objects
    //    var connectionString = context.ConnectionString;
    //    using var conn = new SqlConnection(connectionString);
    //    using var cmd = conn.CreateCommand();

    //    cmd.CommandText = @"
    //        /* Values */
    //        SELECT  v.EntityId, v.AttributeId, a.StaticName, v.Value, v.ValueId
    //        FROM    @EntityIds AS e
    //        JOIN    dbo.TsDynDataValue  AS v ON v.EntityId = e.Id
    //        JOIN    dbo.TsDynDataAttribute AS a ON a.AttributeId = v.AttributeId;

    //        /* Value-Dimensions */
    //        SELECT  vd.ValueId, vd.DimensionId, vd.ReadOnly, d.ExternalKey AS EnvironmentKey
    //        FROM    @EntityIds AS e
    //        JOIN    dbo.TsDynDataValue           AS v  ON v.EntityId   = e.Id
    //        JOIN    dbo.TsDynDataValueDimension  AS vd ON vd.ValueId   = v.ValueId
    //        JOIN    dbo.TsDynDataDimension       AS d  ON d.DimensionId = vd.DimensionId";

    //    cmd.CommandType = CommandType.Text;
    //    cmd.Parameters.Add(tvp);

    //    var results = new List<ValueResult>();
    //    var lookup = new Dictionary<int, ValueResult>();

    //    conn.Open();
    //    using var rdr = cmd.ExecuteReader();

    //    while (rdr.Read())
    //    {
    //        var vr = new ValueResult
    //        {
    //            EntityId = rdr.GetInt32(0),
    //            AttributeId = rdr.GetInt32(1),
    //            StaticName = rdr.GetString(2),
    //            Value = rdr.GetString(3),
    //            ValueId = rdr.GetInt32(4)
    //        };
    //        lookup[vr.ValueId] = vr;
    //        results.Add(vr);
    //    }

    //    if (rdr.NextResult())
    //    {
    //        while (rdr.Read())
    //        {
    //            var child = new ValueDimensionResult
    //            {
    //                ValueId = rdr.GetInt32(0),
    //                DimensionId = rdr.GetInt32(1),
    //                ReadOnly = rdr.GetBoolean(2),
    //                EnvironmentKey = rdr.GetString(3)
    //            };

    //            if (lookup.TryGetValue(child.ValueId, out var parent)) 
    //                parent.ValueDimensionResults.Add(child);
    //        }
    //    }

    //    return results;
    //}
    ///// <summary>
    ///// Builds a SqlParameter for the TVP type [dbo].[TsDynDataIntList] (one int column).
    ///// </summary>
    ///// <param name="parameterName">The name you used in the T-SQL command (e.g. "@EntityIds").</param>
    ///// <param name="ids">Any enumerable of int IDs.</param>
    ///// <returns>A ready-to-use SqlParameter with SqlDbType.Structured.</returns>
    //private static SqlParameter BuildIntListParameter(string parameterName, IEnumerable<int> ids)
    //{
    //    if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
    //    if (!parameterName.StartsWith("@")) parameterName = "@" + parameterName;

    //    // 1  Create a DataTable with the same single column as [dbo].[TsDynDataIntList]
    //    var table = new DataTable();
    //    table.Columns.Add("Id", typeof(int));  // column name must match the TVP definition

    //    // 2  Fill it
    //    foreach (var id in ids ?? []) 
    //        table.Rows.Add(id);

    //    // 3  Wrap it in a structured SqlParameter
    //    return new SqlParameter(parameterName, SqlDbType.Structured)
    //    {
    //        Direction = ParameterDirection.Input,
    //        TypeName = "[dbo].[TsDynDataIntList]",
    //        Value = table
    //    };
    //}

    //// POCO Parent
    //internal class ValueResult
    //{
    //    public int EntityId { get; set; }
    //    public int AttributeId { get; set; }
    //    public string StaticName { get; set; }
    //    public string Value { get; set; }
    //    public int ValueId { get; set; }
    //    public List<ValueDimensionResult> ValueDimensionResults { get; set; } = [];
    //}

    //// POCO Child
    //internal class ValueDimensionResult
    //{
    //    public int ValueId { get; set; }
    //    public int DimensionId { get; set; }
    //    public bool ReadOnly { get; set; }
    //    public string EnvironmentKey { get; set; }
    //}

    #endregion
}