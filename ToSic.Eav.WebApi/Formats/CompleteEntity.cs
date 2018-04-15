using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.WebApi.Formats
{
    public class EntityWithLanguages
    {
        public int Id;
        [NonSerialized] public Guid Guid;

        public Type Type;
        public bool IsPublished;
        public bool IsBranch;
        public string TitleAttributeName;
        public Dictionary<string, Attribute> Attributes;
        public int AppId;

        public static EntityWithLanguages Build(int appId, IEntity entity)
        {
            var ce = new EntityWithLanguages
            {
                AppId = appId,
                Id = entity.EntityId,
                Guid = entity.EntityGuid,
                Type = new Type { Name = entity.Type.Name, StaticName = entity.Type.StaticName },
                IsPublished = entity.IsPublished,
                IsBranch = !entity.IsPublished && entity.GetPublished() != null,
                TitleAttributeName = entity.Title?.Name,
                Attributes = entity.Attributes.ToDictionary(a => a.Key, a => new Attribute
                    {
                        Values = a.Value.Values?.Select(v => new ValueSet
                        {
                            Value = v.SerializableObject,
                            Dimensions = v.Languages.ToDictionary(l => l.Key, y => y.ReadOnly)
                        }).ToArray() ?? new ValueSet[0]
                    }
                )
            };
            return ce;
        }
    }

    public class Attribute
    {
        public ValueSet[] Values;
    }

    public class ValueSet
    {
        public object Value;
        public Dictionary<string, bool> Dimensions;
    }

    public class Type
    {
        public string Name;
        public string StaticName;
    }

    public class Metadata
    {
        /// <summary>
        /// Will return true if a target-type was assigned
        /// </summary>
        public bool HasMetadata => _targetType != Constants.NotMetadata;

        private int _targetType = Constants.NotMetadata;
        /// <summary>
        /// This is the AssignmentObjectTypeId - usually 1 (none), 2 (attribute), 4 (entity)
        /// </summary>
        public int TargetType {
            get => _targetType;
            set
            {
                if(value > 10)
                    throw new Exception("Unexpected TargetType (AssigmentObjectTypeId) - currently only expecting 1-4 or 10, got something else");
                _targetType = value;
            }
        }


        private string _keyType;

        public string KeyType
        {
            get => _keyType;
            set
            {
                var newType = value.ToLower();
                if (newType == "string" || newType == "number" || newType == "guid")
                    _keyType = newType;
                else
                    throw new Exception("metadata keytype unknown. expected number, guid or string" );
            } 
        }

        public string Key { get; set; }

        /// <summary>
        /// The Keynumber is null or the int of the key as stored in "Key"
        /// </summary>
        public int? KeyNumber
        {
            get
            {
                if (KeyType != "number")
                    return null;
                if (!int.TryParse(Key, out int keyNum))
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Tried to retrieve a number-key for the meadata but conversion failed" });
                return keyNum;
            }
        }

        /// <summary>
        /// The KeyGuid is null or the guid of the key as stored in "Key"
        /// </summary>
        public Guid? KeyGuid
        {
            get
            {
                if (KeyType != "guid")
                    return null;
                if (!Guid.TryParse(Key, out var keyGuid))
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Tried to retrieve a guid-key for the meadata but conversion failed" });
                return keyGuid;
            }
        }

        /// <summary>
        /// The KeyString is null or the string of the key as stored in "Key"
        /// </summary>
        public string KeyString => KeyType != "string" ? null : Key;
    }
}
