namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// Represents a Dimension
    /// </summary>
    public interface IDimension
	{
		/// <summary>
		/// Gets the internal DimensionId
		/// </summary>
		int DimensionId { get; }

		/// <summary>
		/// Gets the External Key, for languages usually values like en-US or de-DE
		/// </summary>
		string Key { get; }

		/// <summary>
		/// Gets whether Dimension is assigned read only
		/// </summary>
		bool ReadOnly { get; }
	}
}