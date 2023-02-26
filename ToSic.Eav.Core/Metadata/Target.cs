using System;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <inheritdoc />
    [PublicApi_Stable_ForUseInYourCode]
    public class Target : ITarget
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public bool IsMetadata => TargetType != (int)TargetTypes.None;

        /// <inheritdoc/>
        public int TargetType { get; set; } = (int)TargetTypes.None;

        /// <inheritdoc/>
        public int? KeyNumber { get; set; }

        /// <inheritdoc/>
        public Guid? KeyGuid { get; set; }

        /// <inheritdoc/>
        public string KeyString { get; set; }


        /// <summary>
        /// Constructor for a new MetadataTarget, which is empty.
        /// </summary>
        public Target() { }

        /// <summary>
        /// Constructor for a new MetadataTarget, which is empty.
        /// </summary>
        [PrivateApi]
        public Target(int targetType, string identifier)
        {
            TargetType = targetType;
            Title = identifier;
        }

        /// <summary>
        /// Constructor to copy an existing MetadataFor object. 
        /// </summary>
        /// <param name="originalToCopy"></param>
        [PrivateApi("not sure if this should be public, since we don't have a proper cloning standard")]
        public Target(ITarget originalToCopy, Guid? keyGuid = default)
        {
            TargetType = originalToCopy.TargetType;
            KeyString = originalToCopy.KeyString;
            KeyNumber = originalToCopy.KeyNumber;
            KeyGuid = keyGuid ?? originalToCopy.KeyGuid;
            Title = originalToCopy.Title;
        }

        [JsonIgnore]
        [PrivateApi("WIP v13")] public string Title { get; set; }
        public string[] Recommendations { get; set; } = Array.Empty<string>();
    }
}