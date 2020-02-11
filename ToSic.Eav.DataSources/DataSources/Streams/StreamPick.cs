﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that returns a stream by the provided name. Usually this will be configured through [Params:SomeName]
	/// BETA - this is introduced in 10.25.03 but not officially released yet.
	/// </summary>
    [PrivateApi("not ready for public use")]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.StreamPick, ToSic.Eav.DataSources",
        Type = DataSourceType.Logic,
        ExpectsDataOfType = "67b19864-df6d-400b-9f37-f41f1dd69c4a",
        DynamicOut = false, 
	    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-StreamPick")]

    public sealed class StreamPick: DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.StmPck";

        private const string StreamNameKey = "StreamName";
        private const string SearchInParentKey = "UseParentStreams";

		/// <summary>
		/// The stream name to lookup.
		/// </summary>
		public string StreamName
		{
            get => Configuration[StreamNameKey];
            set => Configuration[StreamNameKey] = value;
        }

		/// <summary>
		/// The attribute whose value will be sorted by.
		/// </summary>
		/// <remarks>
		/// This feature has not been fully implemented yet. The idea would be that it could access an "App" or similar and dynamically access streams from there
		/// This may also have a security risk, so don't finish till this is clarified
		/// </remarks>
		public bool UseParentStreams
		{
            get => bool.TryParse(Configuration[SearchInParentKey], out var result) && result;
            set => Configuration[SearchInParentKey] = value.ToString();
        }

		#endregion


        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityIdFilter
        /// </summary>
        [PrivateApi]
		public StreamPick()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
            ConfigMask(StreamNameKey, "[Settings:StreamName||Default]");
            ConfigMask(SearchInParentKey, "[Settings:UseParent||False]");
		}

		private IEnumerable<IEntity> GetList()
        {
            var wrapLog = Log.Call();
            Configuration.Parse();
            var name = StreamName;
            Log.Add($"StreamName to Look for: '{name}'");
			if(string.IsNullOrWhiteSpace(StreamName)) return new List<IEntity>();
            name = name.ToLowerInvariant();
            var foundStream = In.FirstOrDefault(pair => pair.Key.ToLowerInvariant() == name);
            if (foundStream.Key == string.Empty)
            {
                var msg = $"StreamPick can't find stream by the name '{StreamName}'";
                Log.Add(msg);
                wrapLog("error");
                throw new Exception(msg);
            }

            wrapLog("ok");
            return foundStream.Value.List;
        }

	}
}