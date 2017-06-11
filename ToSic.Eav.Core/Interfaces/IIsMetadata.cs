using System;

namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Dimension
	/// </summary>
	public interface IIsMetadata
	{
        // 2017-06-11 try to disable this completely, I'm assuming we only used it internally
        //[Obsolete("don't use has any more, it's the worng name, should be IsMetadata")]
        //bool HasMetadata { get; }

        bool IsMetadata { get; }

        int TargetType { get; }

        string KeyString { get; }

        int? KeyNumber { get; }

        Guid? KeyGuid { get; }

    }

}