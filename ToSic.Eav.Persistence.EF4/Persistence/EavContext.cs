using System;
using System.Data.Objects;

namespace ToSic.Eav
{
	public partial class EavContext
    {

        #region new save delegates

	    public delegate int OriginalSaveChangesEvent(SaveOptions options);
        public delegate int HandleSaveChangesEvent(SaveOptions options, OriginalSaveChangesEvent baseEvent);

	    public HandleSaveChangesEvent AlternateSaveHandler;

        #endregion

        #region Save and check if to kill cache
        /// <summary>
		/// Persists all updates to the data source and optionally resets change tracking in the object context.
		/// Also Creates an initial ChangeLog (used by SQL Server for Auditing).
		/// If items were modified, Cache is purged on current Zone/App
		/// </summary>
		public override int SaveChanges(SaveOptions options)
        {
            if (AlternateSaveHandler != null)
                return AlternateSaveHandler(options, base.SaveChanges);
            throw new Exception("the save should never happen without an alternate save handler registered");
		}
		#endregion

    }

}