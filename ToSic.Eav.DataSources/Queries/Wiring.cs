using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ToSic.Eav.DataSources.Queries
{
	/// <summary>
	/// Helper for DataPipeline Wiring of DataSources
	/// </summary>
	public static class QueryWiring
	{
		private static readonly Regex WireRegex = new Regex("(?<From>.+):(?<Out>.+)>(?<To>.+):(?<In>.+)", RegexOptions.Compiled);

		/// <summary>
		/// Deserialize a string of Wiring Infos to WireInfo Objects
		/// </summary>
		public static IEnumerable<WireInfo> Deserialize(string wiringsSerialized)
		{
			if (string.IsNullOrWhiteSpace(wiringsSerialized)) return null;

			var wirings = wiringsSerialized.Split(new[] { "\r\n" }, StringSplitOptions.None);

			return wirings.Select(wire => WireRegex.Match(wire)).Select(match => new WireInfo
			{
				From = match.Groups["From"].Value,
				Out = match.Groups["Out"].Value,
				To = match.Groups["To"].Value,
				In = match.Groups["In"].Value
			});
		}

		/// <summary>
		/// Serialize Wire Infos to a String
		/// </summary>
		public static string Serialize(IEnumerable<WireInfo> wirings) => string.Join("\r\n", wirings.Select(w => w.ToString()));
	}


	/// <summary>
	/// Represent a Wire which connects DataSources in a Pipeline
	/// </summary>
	public struct WireInfo
	{
		public string From { get; set; }
		public string Out { get; set; }
		public string To { get; set; }
		public string In { get; set; }

		public override string ToString() => From + ":" + Out + ">" + To + ":" + In;
	}
}