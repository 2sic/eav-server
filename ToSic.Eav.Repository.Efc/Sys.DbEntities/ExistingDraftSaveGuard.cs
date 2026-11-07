using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

// Runs inside the transaction owned by the ordinary save pipeline, before any publishing/value mutation.
internal static class ExistingDraftSaveGuard
{
    public static void RequireOwnedTransaction(DbStorage.DbStorage storage, ICollection<IEntityPair<SaveOptions>> pairs)
    {
        if (!pairs.Any(p => p.Partner.ExistingDraftCondition != null))
            return;
        // WorkEntitySave publishes its cache update after Save returns. An enclosing transaction
        // would move that publication before the caller's commit, so conditional saves refuse it.
        if (System.Transactions.Transaction.Current != null || storage.SqlDb.Database.CurrentTransaction != null)
            throw new NotSupportedException("Conditional draft saves must own the native transaction.");
    }

    public static void Demand(DbStorage.DbStorage storage, ICollection<IEntityPair<SaveOptions>> pairs)
    {
        var guarded = pairs.Where(p => p.Partner.ExistingDraftCondition != null).ToArray();
        if (guarded.Length == 0)
            return;
        if (pairs.Count != 1)
            throw new NotSupportedException("Conditional draft saves require one entity.");
        var pair = guarded[0];
        var expected = pair.Partner.ExistingDraftCondition!;
        if (expected.AppId != storage.AppId || expected.EntityId <= 0 || expected.DraftId <= 0 ||
            expected.EntityId == expected.DraftId || expected.EntityGuid == Guid.Empty ||
            expected.PublishedVersion < 1 || expected.DraftVersion < 1 || pair.Partner.DraftShouldBranch ||
            pair.Entity.EntityId != expected.EntityId || pair.Entity.EntityGuid != expected.EntityGuid || pair.Entity.IsPublished)
            throw new ArgumentException("Conditional save requires an explicit existing draft identity and revisions.");
        var db = storage.SqlDb;
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.SqlServer")
            throw new NotSupportedException("Conditional draft saves currently require SQL Server.");
        var transaction = db.Database.CurrentTransaction
            ?? throw new InvalidOperationException("The draft revision guard requires the native save transaction.");
        var model = db.Model.FindEntityType(typeof(TsDynDataEntity))!;
        string Quote(string identifier) => "[" + identifier.Replace("]", "]]") + "]";
#if NETFRAMEWORK
        var table = Quote(model.Relational().Schema ?? "dbo") + "." + Quote(model.Relational().TableName);
#else
        var table = Quote(model.GetSchema() ?? "dbo") + "." + Quote(model.GetTableName()!);
#endif
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        // Row/range locks also serialize ordinary SQL writers; no MCP-specific application lock is used.
        command.CommandText = "SELECT EntityId,EntityGuid,AppId,Version,IsPublished,PublishedEntityId,TransDeletedId,ContentTypeId FROM " + table +
            " WITH (UPDLOCK,HOLDLOCK) WHERE AppId=@app AND (EntityId=@published OR EntityId=@draft) ORDER BY EntityId";
        foreach (var argument in new[] { ("@app", expected.AppId), ("@published", expected.EntityId), ("@draft", expected.DraftId) })
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = argument.Item1;
            parameter.DbType = DbType.Int32;
            parameter.Value = argument.Item2;
            command.Parameters.Add(parameter);
        }
        var rows = new List<TsDynDataEntity>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
                rows.Add(new TsDynDataEntity { EntityId = reader.GetInt32(0), EntityGuid = reader.GetGuid(1), AppId = reader.GetInt32(2),
                    Version = reader.GetInt32(3), IsPublished = reader.GetBoolean(4), PublishedEntityId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    TransDeletedId = reader.IsDBNull(6) ? null : reader.GetInt32(6), ContentTypeId = reader.GetInt32(7), Owner = "" });
        }
        if (!Matches(expected, rows))
            throw new DBConcurrencyException("The published/draft pair changed before conditional save.");
    }

    internal static bool Matches(ExistingDraftSaveCondition expected, IReadOnlyList<TsDynDataEntity> rows)
    {
        if (rows.Count != 2)
            return false;
        var published = rows.SingleOrDefault(r => r.EntityId == expected.EntityId);
        var draft = rows.SingleOrDefault(r => r.EntityId == expected.DraftId);
        return published != null && draft != null && published.AppId == expected.AppId && draft.AppId == expected.AppId &&
               published.EntityGuid == expected.EntityGuid && draft.EntityGuid == expected.EntityGuid &&
               published.TransDeletedId == null && draft.TransDeletedId == null &&
               published.IsPublished && published.PublishedEntityId == null && !draft.IsPublished &&
               draft.PublishedEntityId == published.EntityId && draft.ContentTypeId == published.ContentTypeId &&
               published.Version == expected.PublishedVersion && draft.Version == expected.DraftVersion;
    }
}
