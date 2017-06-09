namespace ToSic.Eav.Interfaces
{
	/// <summary>
	/// Represents a Value
	/// </summary>
	/// <typeparam name="T">Type of the actual Value</typeparam>
	public interface IValue<T> : IValue
	{
		/// <summary>
		/// Typed contents
		/// </summary>
		T TypedContents { get; }
	}
}
