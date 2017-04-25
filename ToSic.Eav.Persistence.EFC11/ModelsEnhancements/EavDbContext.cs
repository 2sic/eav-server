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
            if (AlternateSaveHandler != null)
                return AlternateSaveHandler(acceptAllChangesOnSuccess, base.SaveChanges);
            throw new Exception("the save should never happen without an alternate save handler registered");
        }
        #endregion


    }
}