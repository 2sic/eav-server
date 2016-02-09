﻿namespace ToSic.Eav.ImportExport
{

	/// <summary>
	/// Represents a Value
	/// </summary>
	public class ValueToImport
	{
		/// <summary>
		/// Gets or sets the Value
		/// </summary>
		public object Value { get; set; }
		/// <summary>
		/// Gets or sets the internal ValueId
		/// </summary>
		public int? ValueId { get; set; }
		/// <summary>
		/// Gets or sets whether the Value is read only (means shared from another Language)
		/// </summary>
		public bool ReadOnly { get; set; }
	}

}