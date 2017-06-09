namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// provides types this[int/array/string] accessors
	/// </summary>
	/// <typeparam name="T">Type of the Value</typeparam>
	public interface IValueOfDimension<T>
	{
		/// <summary>
		/// Gets the Value for this Language
		/// </summary>
		T this[int languageId] { get; }
		/// <summary>
		/// Gets the Value for this Languages
		/// </summary>
		T this[int[] languageIds] { get; }
		/// <summary>
		/// Gets the Value for this Language
		/// </summary>
		T this[string languageKey] { get; }
		/// <summary>
		/// Gets the Value for this Languages
		/// </summary>
		T this[string[] languageKeys] { get; }
	}
    
}
