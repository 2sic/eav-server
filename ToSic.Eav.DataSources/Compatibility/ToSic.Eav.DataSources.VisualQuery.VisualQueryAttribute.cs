using System;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.VisualQuery
{
	/// <summary>
    /// This is added for compatibility with old code which may still have this attribute.
    /// Otherwise DNN fails to start, as the 
    /// </summary>
    [Obsolete("Use the new VisualQuery attribute instead")]
    public class VisualQueryAttribute: Eav.DataSources.Query.VisualQueryAttribute
    {
    }
}
