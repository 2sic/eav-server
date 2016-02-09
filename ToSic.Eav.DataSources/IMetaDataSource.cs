using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// For accessing metadata from the data source. Mainly used in the Store and Cache-Systems, others probably cannot implement it 
	/// because the supply-pipeline won't have everything needed. 
	/// </summary>
	public interface IMetaDataSource
	{
		/// <summary>
		/// Get assigned Entities for specified assignmentObjectTypeId and key
		/// </summary>
		IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, Guid key, string contentTypeName = null);
		/// <summary>
		/// Get assigned Entities for specified assignmentObjectTypeId and key
		/// </summary>
		IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, string key, string contentTypeName = null);
		/// <summary>
		/// Get assigned Entities for specified assignmentObjectTypeId and key
		/// </summary>
		IEnumerable<IEntity> GetAssignedEntities(int assignmentObjectTypeId, int key, string contentTypeName = null);
	}
}