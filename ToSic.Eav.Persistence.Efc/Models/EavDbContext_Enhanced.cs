

// ReSharper disable once CheckNamespace

namespace ToSic.Eav.Persistence.Efc.Models;

partial class EavDbContext 
{
    public delegate int SaveChangesEvent(bool acceptAllChangesOnSuccess);
    public delegate int HandleSaveChangesEvent(bool acceptAllChangesOnSuccess, SaveChangesEvent baseEvent);

    public HandleSaveChangesEvent AlternateSaveHandler;

    #region Save and check if to kill cache
    /// <inheritdoc />
    /// <summary>
    /// Persists all updates to the data source and optionally resets change tracking in the object context.
    /// Also Creates an initial TransactionId (used by SQL Server for Auditing).
    /// If items were modified, Cache is purged on current Zone/App
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (AlternateSaveHandler != null)
            return AlternateSaveHandler(acceptAllChangesOnSuccess, base.SaveChanges);
        throw new("the save should never happen without an alternate save handler registered");
    }

    internal int SaveChanges(string testingOnly, bool acceptAllChangesOnSuccess)
    {
        if(testingOnly != "testingOnly") throw new("this is meant for testing only");
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    #endregion

    #region potential debugging code if needed - all commented out for now
    //private SaveChangesEvent realBaseEvent;
    //public int SaveChangesAndLogEverything(bool acceptAllChangesOnSuccess)
    //{
    //    ChangeTracker.DetectChanges();

    //    //updateUpdatedProperty<SourceInfo>();
    //    //updateUpdatedProperty<DataEventRecord>();

    //    return realBaseEvent(acceptAllChangesOnSuccess);
    //}


    // note: demo code from https://damienbod.com/2016/12/13/ef-core-diagnosis-and-features-with-ms-sql-server/
    //private void updateUpdatedProperty<T>() where T : class
    //{
    //    var modifiedSourceInfo =
    //        ChangeTracker.Entries<T>()
    //            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    //    //foreach (var entry in modifiedSourceInfo)
    //    //    entry.Property("UpdatedTimestamp").CurrentValue = DateTime.UtcNow;
    //}
    #endregion

    #region Test
    // 2025-05-12, stv, test alternative strategy to load values
    //public string ConnectionString => _dbConfig.ConnectionString;

    #endregion
}