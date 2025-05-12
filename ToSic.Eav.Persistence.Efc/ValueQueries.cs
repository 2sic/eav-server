//#if NETFRAMEWORK
//using System.Data.SqlClient;
//#else
//using Microsoft.Data.SqlClient;
//#endif
//using System.Configuration;
//using System.Data;
//using ToSic.Eav.Internal.Configuration;
//using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class ValueQueries(EavDbContext context, ILog parentLog): HelperBase(parentLog, "Efc.ValQry")
{
    // 2025-04-28: this is the old version, which was slower - remove ca. 2025-Q3 #EfcSpeedUpValueLoading
    ///// <remarks>
    ///// Research 2024-08 PC 2dm shows that this is fairly slow, between 100 and 400ms for 1700 attributes (Tutorial App)
    ///// </remarks>
    //internal IQueryable<ToSicEavValues> ValuesOfIdsQuery(List<int> entityIds)
    //{
    //    var l = Log.Fn<IQueryable<ToSicEavValues>>(timer: true);

    //    var query = context.ToSicEavValues
    //        // Note 2025-04-28 2dm: the .Attribute seems to only be used to get the StaticName
    //        // should probably be optimized to not get the Attribute definition for each item (lots of data)
    //        // but instead just get the StaticName
    //        .Include(v => v.Attribute)
    //        // Dimensions are needed for language assignment for each value
    //        .Include(v => v.TsDynDataValueDimensions)
    //        .ThenInclude(d => d.Dimension)
    //        // Skip values which have been flagged as deleted
    //        .Where(v => !v.TransDeletedId.HasValue);

    //    var queryOfEntityIds = query
    //        .Where(r => entityIds.Contains(r.EntityId));

    //    return l.Return(queryOfEntityIds);
    //}

    /// <remarks>
    /// Improved 2025-04-28 for v20 to really just get the values we need, seems to be ca. 50% faster.
    /// </remarks>
    internal IQueryable<TsDynDataValue> ValuesOfIdsQueryOptimized(List<int> entityIds)
    {
        var l = Log.Fn<IQueryable<TsDynDataValue>>(timer: true);

        var query = context.TsDynDataValues;
            // Skip values which have been flagged as deleted
            //.Where(v => !v.TransDeletedId.HasValue);

        var queryOfEntityIds = query
            .Where(r => entityIds.Contains(r.EntityId));

        return l.Return(queryOfEntityIds);
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