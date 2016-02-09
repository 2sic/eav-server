using System;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Dimension
	/// </summary>
	public interface IMetadata
	{
        bool HasMetadata { get; }

        int TargetType { get; }

        string KeyString { get; }

        int? KeyNumber { get; }

        Guid? KeyGuid { get; }

    }

}