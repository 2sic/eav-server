using System;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents an object which is metadata for some other object
	/// </summary>
	public interface IIsMetadata
	{
        bool IsMetadata { get; }

        int TargetType { get; }

        string KeyString { get; }

        int? KeyNumber { get; }

        Guid? KeyGuid { get; }

    }

}