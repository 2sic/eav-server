using System;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class EavDbContext 
    {
        public delegate int SaveChangesEvent(bool acceptAllChangesOnSuccess);
        public delegate int HandleSaveChangesEvent(bool acceptAllChangesOnSuccess, SaveChangesEvent baseEvent);

        public HandleSaveChangesEvent AlternateSaveHandler;

        #region Save and check if to kill cache
        /// <summary>
		/// Persists all updates to the data source and optionally resets change tracking in the object context.
		/// Also Creates an initial ChangeLog (used by SQL Server for Auditing).
		/// If items were modified, Cache is purged on current Zone/App
		/// </summary>
		public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            //realBaseEvent = base.SaveChanges;
            if (AlternateSaveHandler != null)
                return AlternateSaveHandler(acceptAllChangesOnSuccess, base.SaveChanges /*SaveChangesAndLogEverything*/);
            throw new Exception("the save should never happen without an alternate save handler registered");
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
    }
}