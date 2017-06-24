using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a map to sub-items, based on the metadata given
	/// </summary>
	public interface IHasMetadata
	{
        bool HasMetadata { get; }

        List<IEntity> Items { get; }

    }

}